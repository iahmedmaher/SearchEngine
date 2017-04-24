<?php 

class ListSearch
{
	protected $query;
	protected $conn;
	protected $hassteps;
	
	protected $head;
	protected $listitems;
	protected $sourcetitle;
	protected $sourcelink;
	
	public function __construct($q)
	{
		$this->conn = new sqlite3("Index.db"); 
		$this->query=$this->conn->escapeString($q);
	}
	
	public function excute()
	{
		$sql="SELECT U.URL,U.Title ,S.Header, S.List FROM StepsSuggestions S,URL U WHERE S.Header MATCH '".$this->query."' AND U.ID=S.LID ORDER BY hex(matchinfo(StepsSuggestions,'x')) desc";
		$res=$this->conn->query($sql);
		//count
		
		$this->hassteps=true;
		
		$res=$res->fetchArray();
		
		$this->head=$res["Header"];
		$this->listitems=json_decode($res["List"]);
		$this->sourcelink=$res["URL"];
		$this->sourcetitle=$res["Title"];
	}
	
	public function HasList()
	{
		return $this->hassteps;
	}
	
	public function getHeader()
	{
		return $this->head;
	}
	
	public function getList()
	{
		return $this->listitems;
	}
	
	public  function getPageTitle()
	{
		return $this->sourcetitle;
	}
	
	public function getPageUrl()
	{
		return $this->sourcelink;
	}
	
	function __destruct()
	{
		$this->conn->close();	
	}
	
}


?>