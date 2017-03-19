<?php 
if(empty($_GET['q'])){
	header("Location: index.html");
	die();
}
$user_query=$_GET['q'];
require_once("search.inc.php");
?>
<!doctype html>
<html>
<head>

<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1">

<title>
<?php echo htmlentities($user_query) ?> - KEMO'S Search Engine
</title>

<link href="css/search.css" rel="stylesheet" type="text/css">
<link href="css/suggestions.css" rel="stylesheet" type="text/css">
<link href="css/results.css" rel="stylesheet" type="text/css">
<script src="js/jquery.min.js"></script>
<script src="js/suggestions.js"></script>

</head>
<body>
<div id="res-searchbar-div">
<div class="results-search-bar">
<form class="search_bar large" action="search.php" method="GET" id="search-form">
  <input type="text" placeholder="Search KEMO's for anything" name="q" id="query-text" autocomplete="off" value="<?php echo $user_query?>"/>
  <button type="submit" value="Search">Search</button>
  <div class="autocomplete-suggestions" style="position: absolute; width: 90%; max-height: 300px; z-index: 9999; display: none;"></div>
</form>
</div>
</div>

<div id="results-area">

<div id="meta">
<i>About <?php echo res_count() ?> results in <?php echo 1 ?> seconds.</i>
</div>

<div class="single-result">
<a href="#"><h3>test</h3></a>
<cite>http://karimashraf.tk</cite>
<br />
<span class="date">5/5/2015 - </span>
<span> 
Simple test simple test simple
</span>
</div>

</div>

</body>
</html>