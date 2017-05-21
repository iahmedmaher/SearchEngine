<?php

class Database
{
	private static $singleton=null;
	private $conn;
	
	private function __construct()
	{
		$this->conn = new sqlite3("Index.db"); 
	}
	
	public static function getInstance()
	{
		if(self::$singleton==null)
			self::$singleton=new Database();
		return self::$singleton;
	}
	
	public function query($sql)
	{
		return $this->conn->query($sql);
	}
	
	public function escapeString($str)
	{
		return $this->conn->escapeString($str);
	}
	
	public function __destruct()
	{
		$this->conn->close();	
	}
}

?>