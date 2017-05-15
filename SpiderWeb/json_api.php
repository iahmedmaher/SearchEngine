<?php
include("search.inc.php");

$user_query=$_GET["q"];
$page=$_GET["page"];

$s = new NormalSearcher($user_query,$page);

$s_arr = array();

$result = $s->excute();

while($row = $result->fetchArray(SQLITE3_ASSOC))
{
	$content=str_replace("<b>","",$s->GetContentByIDMarked($row["ID"],$user_query));
	$content=str_replace("</b>","",$content);
	$content=str_replace("\r\n"," ",$content);
	$content=str_replace("\n"," ",$content);
	$content=str_replace("\t"," ",$content);
	$content=str_replace("  ","",$content);
	$row["content"]=$content;
	$s_arr[]=$row;
}

//,JSON_PRETTY_PRINT 
echo json_encode($s_arr);

?>