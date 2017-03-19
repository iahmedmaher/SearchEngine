<?php 

if(!isset($_GET["q"]))
{die();}

$q=$_GET["q"];

// create curl resource 
$ch = curl_init(); 

curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
curl_setopt($ch, CURLOPT_IPRESOLVE, CURL_IPRESOLVE_V4);

// set url 
curl_setopt($ch, CURLOPT_URL, "https://en.wikipedia.org/w/api.php?action=opensearch&format=json&formatversion=2&search=".urlencode($q)."&namespace=0&limit=10&suggest=true"); 

//return the transfer as a string 
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1); 

// $output contains the output string 
$output = curl_exec($ch); 

echo curl_error($ch);

// close curl resource to free up system resources 
curl_close($ch);     

echo $output;
?>