<?php
class Searcher
{
	protected $page;
	protected $Rcount;
	
	protected $exactphrase;
	protected $contains;
	protected $doesntcontains;
	protected $nearwords;
	protected $neardist;
	
	protected $normalquery;
	
	protected $conn;
	
	
	public function excute()
	{
		
	}
	
	public function GetCount()
	{
		return $this->Rcount;
	}
	
	function __destruct()
	{
		$this->conn->close();	
	}
	
};

class AdvancedSearcher extends Searcher
{
	public function __construct($exactphrase,$contains,$doesntcontains,$nearwords,$neardist,$page=0)
	{
		$this->conn = new sqlite3("Index.db"); 
		$this->exactphrase=$exactphrase;
		$this->contains=$contains;
		$this->doesntcontains=$doesntcontains;
		$this->nearwords=$nearwords;
		$this->neardist=$neardist;
		$this->page=$page;
	}
	
	public function excute()
	{
		$sql1="SELECT URL.URL,URL.Title,URL.TIMESTAMP,PageContent.Content FROM URL,PageContent WHERE URL.ID=PageContent.LID AND Content MATCH '";
		
		$sql2="SELECT count(*) AS C FROM URL,PageContent WHERE URL.ID=PageContent.LID AND Content MATCH '";
		
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
		
		$sql=$sql."' ORDER BY hex(matchinfo(PageContent,'x')) DESC LIMIT 10 OFFSET ". $this->page*10 .";";
		
		$this->Rcount=$this->conn->query($sql2.$sql)->fetchArray()['C'];
 
		//echo $sql1.$sql;
		//$x=$conn->escapeString($x);
		return $this->conn->query($sql1.$sql);	
	}
};


class NormalSearcher extends Searcher
{
	public function __construct($userquery,$page=0)
	{
		$this->conn = new sqlite3("Index.db"); 
		$this->normalquery=$userquery;
		$this->page=$page;
		$this->neardist=4;
	}
	
	public function excute()
	{
		$sql1="SELECT URL.URL,URL.Title,URL.TIMESTAMP,PageContent.Content FROM URL,PageContent WHERE URL.ID=PageContent.LID AND Content MATCH '";
		
		$sql2="SELECT count(*) AS C FROM URL,PageContent WHERE URL.ID=PageContent.LID AND Content MATCH '";
		
		$sql="";
		
		if($this->nearwords!="")
		{
			$near_arr=explode(" ",$this->normalquery);
			for($i=0;$i<sizeof($near_arr)-1;$i++)
				$sql=$sql.$near_arr[$i].' NEAR/'.$this->neardist.' ';
			
			$sql=$sql.end($near_arr);
		}
		
		$sql=$sql."' ORDER BY hex(matchinfo(PageContent,'x')) DESC LIMIT 10 OFFSET ". $this->page*10 .";";
		
		$this->Rcount=$this->conn->query($sql2.$sql)->fetchArray()['C'];
 
		//echo $sql1.$sql;
		//$x=$conn->escapeString($x);
		return $this->conn->query($sql1.$sql);	
	}
};
?>