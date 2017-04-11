<?php 

if(!isset($_GET["q"]))
{die();}

header('Content-Type: application/json');

$q=$_GET["q"];

$rows = array();
$results = null;

if(strlen($q)>2){

	$db = new SQLite3(strtoupper($q[0]).'.db');


	$results = $db->query("SELECT DISTINCT Suggestion FROM Suggestions WHERE Suggestion like '".$q."%' ORDER BY Rank LIMIT 50");
	while ($row = $results->fetchArray()) {
		$rows[] = $row["Suggestion"];
	}

}
echo json_encode($rows);

?>