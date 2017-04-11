<?php
header('Content-Type: application/json');
include("search.inc.php");

$user_query=$_GET["q"];
$page=$_GET["page"];

$s = new NormalSearcher($user_query,$page);

$s_arr = array();

$result = $s->excute();

while($row = $result->fetchArray(SQLITE3_ASSOC))
{
	$s_arr[]=$row;
}

//,JSON_PRETTY_PRINT 
echo json_encode($s_arr);

?>