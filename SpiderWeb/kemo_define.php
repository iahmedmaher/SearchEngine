<?php

class definer{
	
	private $searchtext;
	private $ambiguous;
	private $found;
	private $result;
	private $result_title;
	private $summary;
	
	public function __construct($s)
	{
		$this->searchtext=$s;
		$this->ambiguous=false;
		$this->found=false;
	}
	
	public function excute()
	{
		$q=$this->searchtext." (disambiguation)";
		$api_link="https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=&explaintext=&titles=".urlencode($q)."&redirects=1&format=json";

		$s = $this->getpage($api_link);

		if($s===null)
		{
			$this->found=false;
			return;
		}
		
		if(key($s)!=-1) //key of the first element
		{
			$this->ambiguous=true;
			//return;
		}
		else {
			$this->ambiguous=false;
		}
		
		$q=$this->searchtext;
		
		$api_link="https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exsentences=4&exintro=&explaintext=&titles=".urlencode($q)."&redirects=1&format=json";

		$s = $this->getpage($api_link);
		
		if($s===null)
		{
			$this->found=false;
			return;
		}
		
		if(key($s)==-1) //key of the first element
		{
			$this->found=false;
			return;
		}
		$this->found=true;
		
		$this->result_title=current($s)["title"];
		$this->result=preg_replace("/\(.+?\)( |,)/","",current($s)["extract"]); //extract usefull info
		
		if(stripos($this->result, 'may refer to') !== false || stripos($this->result_title, '(disambiguation)') !== false)
		{
			$this->found=false;
			$this->ambiguous=true;
			return;
		}
		
		$str = substr($this->result,0,255); 
		$pos = strrpos($str,'.')+1;
		
		if($pos>1)
			$this->summary = substr($str,0,$pos);
		else
		{
			$str = substr($this->result,0,500); 
			$pos = strrpos($str,'.')+1;
			if($pos>1)
				$this->summary = substr($str,0,$pos);
			else
				$this->found = false;
		}
	}
	
	public function is_ambiguous()
	{
		return $this->ambiguous;
	}
	
	public function has_definition()
	{
		return $this->found;
	}
	
	public function getTitle()
	{
		return $this->result_title;
	}
	
	public function getSummary()
	{
		return $this->summary;
	}
	
	private function getpage($url)
	{
		$ch = curl_init($url);
		curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, 0);
		curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, 0);
		curl_setopt ($ch, CURLOPT_RETURNTRANSFER, 1);
		curl_setopt ($ch, CURLOPT_USERAGENT, "KEMOS"); // required by wikipedia.org server; use YOUR user agent with YOUR contact information. (otherwise your IP might get blocked)
		curl_setopt($ch, CURLOPT_TIMEOUT, 10);
		$c = curl_exec($ch);

		$json = json_decode($c,true);

		return $json['query']['pages'];
	}
}


?>