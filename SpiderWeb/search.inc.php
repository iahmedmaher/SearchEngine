<?php
require_once("porter.inc.php");

class Searcher
{
	protected $page;
	protected $Rcount;
	protected $conn;

	public function __construct()
	{
		$this->conn = new sqlite3("Index.db"); 
	}
	
	public function GetContentByID($LID)
	{
		$sql="SELECT Content FROM PageContent WHERE LID=".$LID;
		return $this->conn->query($sql)->fetchArray()['Content'];
	}
	
	public function GetCount()
	{
		return $this->Rcount;
	}
	
	function __destruct()
	{
		$this->conn->close();	
	}
	
}

class AdvancedSearcher extends Searcher
{
	protected $exactphrase;
	protected $contains;
	protected $doesntcontains;
	protected $nearwords;
	protected $neardist;
	
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
		
		$sql=$sql."'";
		
		$this->Rcount=$this->conn->query($sql2.$sql)->fetchArray()['C'];
		
		$sql=$sql." ORDER BY hex(matchinfo(PageContent,'x')) DESC LIMIT 10 OFFSET ". $this->page*10 .";";
		
		//echo $sql1.$sql;
		//$x=$conn->escapeString($x);
		return $this->conn->query($sql1.$sql);	
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
		Example QUERY (Optimized: LIMIT in the subquery before JOIN)
		SELECT URL.ID,URL.URL,URL.Title,URL.TIMESTAMP FROM URL,(SELECT LID,Sum(Rank) AS R FROM VECTOR WHERE Keyword MATCH 'Visual OR studio' GROUP BY LID ORDER BY Sum(Rank) DESC LIMIT 10 OFFSET 0) AS SUB WHERE  URL.ID=SUB.LID ORDER BY (InBound) DESC, R DESC;
		*/

		$sql_count = "SELECT count(DISTINCT LID) AS C FROM VECTOR WHERE Keyword MATCH '";
		
		$sql="SELECT URL.ID,URL.URL,URL.Title,URL.TIMESTAMP FROM URL,(SELECT LID,Sum(Rank) AS R FROM VECTOR WHERE Keyword MATCH '";
		
		
			$sqlcond="";
		
			$near_arr=explode(" ",$this->normalquery);
			for($i=0;$i<sizeof($near_arr)-1;$i++)
				if(strlen(trim($near_arr[$i]))>0)
					$sqlcond=$sqlcond."".PorterStemmer::Stem(strtolower($near_arr[$i]))." OR ";
			
			$sqlcond=$sqlcond.PorterStemmer::Stem(strtolower(end($near_arr)))."'";

			
		$sql_count=$sql_count.$sqlcond;

		$this->Rcount=$this->conn->query($sql_count)->fetchArray()['C'];

		$totalCount=$this->conn->query("SELECT Count(ID) AS C FROM URL")->fetchArray()['C'];
		
		$idf = log10(floatval($totalCount)/(1+floatval($this->Rcount)));
		
		$sql=$sql.$sqlcond.' GROUP BY LID ORDER BY Sum(Rank)*'.$idf.' DESC LIMIT 10 OFFSET '. $this->page*10 .') AS SUB WHERE  URL.ID=SUB.LID ORDER BY R desc;';
	
		return $this->conn->query($sql);	
	}
}
?>