<?php 

if(empty($_GET['q']) && !isset($_GET["advanced"])){
	header("Location: index.html");
	die();
}

if(!isset($_GET["advanced"]))
{
	$user_query=$_GET["q"];
}

$time_pre = microtime(true);
require_once("search.inc.php");
?>
<!doctype html>
<html>
<head>

<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1">
<meta charset="UTF-8">
<title>
<?php  echo isset($_GET["advanced"])?"Advanced Search":$user_query ?> - KEMO'S Search Engine
</title>

<link href="css/search.css" rel="stylesheet" type="text/css">
<link href="css/suggestions.css" rel="stylesheet" type="text/css">
<link href="css/results.css" rel="stylesheet" type="text/css">
<script src="js/jquery.min.js"></script>
<script src="https://cdn.jsdelivr.net/jquery.ui/1.11.4/jquery-ui.min.js"></script>
<script src="js/main.js"></script>

</head>
<body>
<div id="res-searchbar-div">
<div class="results-search-bar">
<form class="search_bar large main" action="search.php" method="GET" id="search-form">
  <input type="text" placeholder="Search KEMO's for anything" name="q" id="query-text" autocomplete="off" value="<?php echo isset($_GET["advanced"])?"Advanced Search":$user_query ?>"/>
  <button type="submit" value="Search">Search</button>
  <div class="autocomplete-suggestions" style="position: absolute; width: 90%; max-height: 300px; z-index: 9999; display: none;"></div>
</form>
</div>
</div>

<div id="results-area">

<?php 

if(isset($_GET["page"]))
	$page=$_GET["page"];
else
	$page=0;


$s = null;

if(isset($_GET["advanced"]))
	$s = new AdvancedSearcher($_GET["phrase"],$_GET["contains"],$_GET["ncontains"],$_GET["nearw"],$_GET["neard"],$page);
else
	$s = new NormalSearcher($_GET["q"],$page);

$result = $s->excute();

$time_post = microtime(true);

?>

<div id="meta">
<i>About <?php echo $s->GetCount() ?> results in <?php echo round($time_post-$time_pre,5) ?> seconds.</i>
</div>

<?php
while($row = $result->fetchArray()):
?>

<div class="single-result">
<a href="<?php echo $row["URL"] ?>"><h3><?php echo $row["Title"] ?></h3></a>
<cite><?php echo $row["URL"] ?></cite>
<br />
<span class="date"><?php echo $row["TIMESTAMP"] ?> - </span>
<span> 
<?php echo substr($s->GetContentByID($row["ID"]),0,500) ?>
</span>
</div>

<?php
endwhile;
?>

</div>

</body>
</html>