<?php
require_once("porter.inc.php");
require_once("database.php");

class Searcher
{
	protected $page;
	protected $Rcount;
	protected $conn;

	public function __construct()
	{
		$this->conn = Database::getInstance();
	}
	
	public function GetContentByIDMarked($LID,$query)
	{
		$sql="SELECT ".'snippet(PageContent,"<b>","</b>","...",-1,64)'." FROM PageContent WHERE Content MATCH '".$query."' AND LID=".$LID;
		return $this->conn->query($sql)->fetchArray()[0];
	}
	
	public function GetContentByID($LID)
	{
		$sql="SELECT Content FROM PageContent WHERE LID=".$LID;
		$str=$this->conn->query($sql)->fetchArray()['Content'];
		$str = substr($str,0,500); 
		return substr($str,0,strrpos($str,' '))." ...";
	}
	
	public function GetCount()
	{
		return $this->Rcount;
	}
	
	public function __destruct()
	{
		//$this->conn->close();	
	}
	
}

class AdvancedSearcher extends Searcher
{
	protected $exactphrase;
	protected $contains;
	protected $doesntcontains;
	protected $nearwords;
	protected $neardist;
	private $query;
	
	public function __construct($exactphrase,$contains,$doesntcontains,$nearwords,$neardist,$page=0)
	{
		parent::__construct();
		$this->exactphrase=$this->conn->escapeString($exactphrase);
		$this->contains=$this->conn->escapeString($contains);
		$this->doesntcontains=$this->conn->escapeString($doesntcontains);
		$this->nearwords=$this->conn->escapeString($nearwords);
		$this->neardist=$this->conn->escapeString($neardist);
		$this->page=$this->conn->escapeString($page);
	}
	
	public function excute()
	{
		/*
		Example QUERY 
		SELECT URL.URL,URL.Title,URL.TIMESTAMP,PageContent.Content FROM URL,PageContent WHERE URL.ID=PageContent.LID AND Content MATCH 'Karim NEAR/4 Engineering'
		*/
		
		$sql1="SELECT URL.ID,URL.URL,URL.Title,URL.TIMESTAMP FROM URL,PageContent WHERE URL.ID=PageContent.LID AND Content MATCH '";
		
		$sql2="SELECT count(*) AS C FROM PageContent WHERE Content MATCH '";
		
		$sql="";
		
		if($this->exactphrase!="")
			$sql=$sql.'"'.$this->exactphrase.'"';
		
		if($this->contains!="")
		{
			$contains_arr=explode(" ",$this->contains);
			foreach($contains_arr as $var)
				$sql=$sql.' AND '.$var;
		}
		
		if($this->doesntcontains!="")
		{
			$dcontains_arr=explode(" ",$this->doesntcontains);
			foreach($dcontains_arr as $var)
				$sql=$sql.' AND -'.$var;
		}
		
		$sql=$sql.' ';
		
		if($this->nearwords!="")
		{
			$near_arr=explode(" ",$this->nearwords);
			for($i=0;$i<sizeof($near_arr)-1;$i++)
				$sql=$sql.$near_arr[$i].' NEAR/'.$this->neardist.' ';
			
			$sql=$sql.end($near_arr);
		}
		$this->query=$sql;
		
		$sql=$sql."'";
		
		$this->Rcount=$this->conn->query($sql2.$sql)->fetchArray()['C'];
		
		$sql=$sql." ORDER BY hex(matchinfo(PageContent,'x')) DESC LIMIT 10 OFFSET ". $this->page*10 .";";
		
		//echo $sql1.$sql;
		//$x=$conn->escapeString($x);
		return $this->conn->query($sql1.$sql);	
	}
	
	public function GetContentByIDMarked($LID,$qs)
	{
		$sql="SELECT ".'snippet(PageContent,"<b>","</b>","...",-1,64)'." FROM PageContent WHERE Content MATCH '".$this->query."' AND LID=".$LID;
		return $this->conn->query($sql)->fetchArray()[0];
	}
}


class NormalSearcher extends Searcher
{
	protected $normalquery;
	
	public function __construct($userquery,$page=0)
	{
		parent::__construct();
		$this->normalquery=$this->conn->escapeString($userquery);
		$this->normalquery=preg_replace("/ +?/"," ",$this->normalquery);
		$this->page=$this->conn->escapeString($page);
		$this->neardist=4;
	}
	
	public function excute()
	{
		/*
		Example QUERY (Optimized: LIMIT in the subquery before JOIN, Fixed: Apply idf for each individual term)

		SELECT URL.ID,URL.URL,URL.Title,URL.TIMESTAMP FROM URL,(SELECT LID,(0.5+0.5*Sum(Rank * (CASE Keyword WHEN 'microsoft' THEN 7.5359628770302 WHEN 'visual' THEN 10.511326860841 WHEN 'studio' THEN 8.6844919786096 END))) AS R FROM VECTOR WHERE Keyword MATCH 'microsoft OR visual OR studio' GROUP BY LID ORDER BY R DESC LIMIT 10 OFFSET 0) AS SUB WHERE URL.ID=SUB.LID ORDER BY R desc, length(URL) ASC;

		*/

		$idf_array = array();
		
		$sql_count = "SELECT count(DISTINCT LID) AS C FROM VECTOR WHERE Keyword MATCH '";
		
			$sqlcond="";
		
			$near_arr=explode(" ",$this->normalquery);
			for($i=0;$i<sizeof($near_arr)-1;$i++)
			{
				if(strlen(trim($near_arr[$i]))>0)
				{
					$sqlcond=$sqlcond."".PorterStemmer::Stem(strtolower($near_arr[$i]))." OR ";
				}
			}
			
			$sqlcond=$sqlcond.PorterStemmer::Stem(strtolower(end($near_arr)))."'";

			
		$sql_count=$sql_count.$sqlcond;

		$this->Rcount=$this->conn->query($sql_count)->fetchArray()['C'];

		$totalCount=$this->conn->query("SELECT Count(ID) AS C FROM URL")->fetchArray()['C'];

		//calculate idf for each word
		foreach($near_arr as $oword)
		{
			$word=PorterStemmer::Stem(strtolower($oword));
			
			$df = $this->conn->query("SELECT Count(DISTINCT LID) AS C FROM VECTOR WHERE Keyword MATCH '".$word."';")->fetchArray()['C'];
			
			$idf_array[$word]=$totalCount/(1+floatval($df));
		}
		
		$sql="SELECT URL.ID,URL.URL,URL.Title,URL.TIMESTAMP FROM URL,(SELECT LID,(0.5+0.5*Sum(Rank * (CASE Keyword";
		
		foreach ($idf_array as $key => $value)
		{
			$sql=$sql." WHEN '".$key."' THEN ". $value;
		}
		
		$sql=$sql." END))) AS R FROM VECTOR WHERE Keyword MATCH '";
		$sql=$sql.$sqlcond.' GROUP BY LID ORDER BY R DESC LIMIT 10 OFFSET '. $this->page*10 .') AS SUB WHERE  URL.ID=SUB.LID ORDER BY R desc, length(URL) ASC , (InBound-OutBound) desc;';//;';
		
		return $this->conn->query($sql);	
	}
}

class ImageSearcher extends Searcher
{
	protected $imagequery;
	
	public function __construct($userquery,$page=0)
	{
		parent::__construct();
		$this->imagequery=$this->conn->escapeString($userquery);
		$this->imagequery=preg_replace("/ +?/"," ",$this->imagequery);
		$this->page=$this->conn->escapeString($page);
		$this->neardist=4;
	}
	
	public function excute()
	{
		$sql = "SELECT URL,Title,TIMESTAMP,ImageLink,ImageAlt FROM URL,Images WHERE URL.ID=Images.LID AND "; 
		$sql_cond="ImageAlt MATCH '".$this->imagequery."'";
		$sql=$sql.$sql_cond." LIMIT 30 OFFSET ".$this->page*30;
		
		$this->Rcount=$this->conn->query("SELECT count(*) AS C FROM Images WHERE ".$sql_cond)->fetchArray()['C'];
		
		return $this->conn->query($sql);	
	}
	
}

?>