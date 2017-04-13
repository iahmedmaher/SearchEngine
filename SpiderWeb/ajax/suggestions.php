<?php 
if(!isset($_GET["q"]))
{die();}

$q=$_GET["q"];

$rows = array();
$results = null;


if(strlen($q)>2){
	if(($q[0]<='Z' and $q[0]>='A') OR ($q[0]<='z' and $q[0]>='a')){
		$db = new SQLite3(strtoupper($q[0]).'.db');

		$results = $db->query("SELECT DISTINCT Suggestion FROM Suggestions WHERE Suggestion like '".$q."%' ORDER BY Rank LIMIT 50");
		while ($row = $results->fetchArray()) {
			$rows[] = $row["Suggestion"];
		}
	}
}
echo json_encode($rows);
?>