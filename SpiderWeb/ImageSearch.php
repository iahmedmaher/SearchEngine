<!doctype html>
<?php 
require_once("search.inc.php");
if(!isset($_GET["q"])){
	header("Location: index.html");
	die();
}
$page;
if(!isset($_GET["page"]))
	$page=0;
else
	$page=$_GET["page"]-1;

$time_pre = microtime(true);

$s = new ImageSearcher($_GET["q"],$page);

$result = $s->excute();

$time_post = microtime(true);
?>
<html>
	<head>
		<title>Image Search</title>
		<link href="css/search.css" rel="stylesheet" type="text/css">
		<link href="css/suggestions.css" rel="stylesheet" type="text/css">
		<link href="css/results.css" rel="stylesheet" type="text/css">
		<link href="css/ImageSearch.css" rel="stylesheet" type="text/css">
		<script src="js/jquery.min.js"></script>
		<script src="js/ImageSearch.js"></script>
	</head>
	<body>
	<div id="loading" style="display:none"></div>
	<div id="res-searchbar-div">
		<div class="results-search-bar">
			<form class="search_bar large main" method="GET" id="search-form">
			  <input type="text" placeholder="Search KEMO's for anything" name="q" id="query-text" autocomplete="off" value="<?php echo $_GET["q"] ?>"/>
			  <button type="submit" value="Search">Search</button>
			  <div class="autocomplete-suggestions" style="position: absolute; width: 90%; max-height: 300px; z-index: 9999; display: none;"></div>
			</form>
		</div>
	</div>
	<div id="meta" style="margin: 8px 16px;">
        <i><?php echo ($page!=0 ? "Page ".($page+1)." of a":"A")?>bout <?php echo $s->GetCount() ?> results in <?php echo round($time_post-$time_pre,3) ?> seconds.</i>
    </div>
	<?php
	
	?>
		<section class="image-grid">
		  <?php 
			$i=0;
			while($row = $result->fetchArray()):
			?>
			
			<article class="image__cell is-collapsed">
			<div class="image--basic">
			<a href="#expand-jump-<?php echo $i ?>">
			<img id="expand-jump-<?php echo $i ?>" class="basic__img" src="<?php echo $row["ImageLink"] ?>" alt="<?php echo $row["ImageAlt"] ?>" />
			</a>
			<div class="arrow--up"></div>
			</div>
			<div class="image--expand">
				<a href="#close-jump-<?php echo $i ?>" class="expand__close"></a>
			  <img class="image--large" src="<?php echo $row["ImageLink"] ?>" alt="<?php echo $row["ImageAlt"] ?>" />
			</div>
		  </article>
			<?php
			$i++;
			endwhile;
		  ?>
		</section>
		
		
<div class="pagination-container">
    <div class="pagination">
        <?php
        $url = $_SERVER["REQUEST_URI"];
		
		if(!isset($_GET["page"]))
	        $url=$url."&page=0";

		$page_n = 10*floor($page/10);
		$upperlimit = floor(($s->GetCount()-1)/30);
		
        if($page!=0):
        $url=preg_replace("/page=\d{1,4}/i","page=".($page),$url);
        ?>
        <a href="<?php echo $url ?>">&laquo;</a>
        <?php endif; ?>  
        <?php
        
		for($i=0;$i<10 and $page_n<=$upperlimit;$i++):
			$url=preg_replace("/page=\d{1,4}/i","page=".($page_n+1),$url);

        ?> 
        <a href="<?php echo $url ?>" <?php echo ($page_n==$page ? 'class="active"':'')?>><?php echo $page_n+1 ?></a>
        <?php 
			$page_n++;
        endfor; 
        ?>
        <?php
        if($page<$upperlimit):
        $url=preg_replace("/page=\d{1,4}/i","page=".($page+2),$url);
        ?>
        <a href="<?php echo $url ?>">&raquo;</a>
        <?php endif; ?> 
    </div>
</div>
		
	</body>
</html>