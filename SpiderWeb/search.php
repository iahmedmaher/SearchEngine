<?php 

require_once("search.inc.php");
require_once("kemo_define.php");
require_once("stopwords.php");

if(empty($_GET['q']) && !isset($_GET["advanced"])){
	header("Location: index.html");
	die();
}

if(!isset($_GET["advanced"]))
{
	$user_query=trim(removeCommonWords($_GET["q"]));
}

if(isset($_GET["page"]))
	$page=$_GET["page"]-1;
else
	$page=0;

$time_pre = microtime(true);

$s = null;

if(isset($_GET["advanced"])){
	$s = new AdvancedSearcher($_GET["phrase"],$_GET["contains"],$_GET["ncontains"],$_GET["nearw"],$_GET["neard"],$page);
}
else{
	$s = new NormalSearcher($user_query,$page);
}

$result = $s->excute();

$time_post = microtime(true);

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

<script type="text/javascript">
$(document).ready(function(){
	$(".pagination a").click(function(){
		$("#loading").show();
		$("body").css({"overflow":"hidden"});
	});
	$("form").submit(function(){
		$("#loading").show();
		$("body").css({"overflow":"hidden"});
	});
});
</script>

</head>
<body>

<div id="loading" style="display:none"></div>

<div id="res-searchbar-div">
	<div class="results-search-bar">
		<form class="search_bar large main" action="search.php" method="GET" id="search-form">
		  <input type="text" placeholder="Search KEMO's for anything" name="q" id="query-text" autocomplete="off" value="<?php echo isset($_GET["advanced"])?"Advanced Search":$_GET["q"] ?>"/>
		  <button type="submit" value="Search">Search</button>
		  <div class="autocomplete-suggestions" style="position: absolute; width: 90%; max-height: 300px; z-index: 9999; display: none;"></div>
		</form>
	</div>
</div>
<div id="results-area">
    <div id="meta">
        <i><?php echo ($page!=0 ? "Page ".($page+1)." of a":"A")?>bout <?php echo $s->GetCount() ?> results in <?php echo round($time_post-$time_pre,3) ?> seconds.</i>
    </div>

	<?php
	if($page==0 && isset($user_query)):
		$defineterm = new definer($user_query);
		$defineterm->excute();
	
		if($defineterm->is_ambiguous()):
	?>
		
		<div class="single-result">
		<h5 id="res-warn">Hint: Your search seems to be too general or/and ambiguous; try more specific search to find more relevant results</h5>
		</div>
	
	<?php
		endif;
		if($defineterm->has_definition()):
    ?>
		<div class="single-result" id="definition-card">
		<h2><?php echo $defineterm->getTitle() ?></h2>
		<p><?php echo $defineterm->getSummary() ?></p>
		</div>
		
	<?php
		endif;
	endif;
    while($row = $result->fetchArray()):
    ?>

    <div class="single-result">
        <a href="<?php echo $row["URL"] ?>" title="<?php echo ($row["Title"]==""?"(No Title)":$row["Title"]) ?>">
            <h3>
                <?php
                if($row["Title"]==""):
	                echo "(No Title)";
                elseif(strlen($row["Title"]) > 60):
	                $str = substr($row["Title"],0,55);
					echo substr($str,0,strrpos($str,' '))." ...";
                else:
	                echo $row["Title"];
                endif;
                ?>
            </h3>
        </a>
        <cite class="ellipses">
            <?php
            if(strlen($row["URL"]) > 100):
	            echo substr($row["URL"],0,100)." ...";
            else:
	            echo $row["URL"];
            endif;
            ?>
        </cite>
        <br />
            <span class="date"><?php echo $row["TIMESTAMP"] ?> - </span>
        <span> 
            <?php 
			$str = $s->GetContentByID($row["ID"]);
			$str = substr($str,0,500); 
			echo substr($str,0,strrpos($str,' '))." ...";
			?>
        </span>
    </div>

    <?php
    endwhile;
    ?>

    <div class="pagination">

        <?php
        $url = $_SERVER["REQUEST_URI"];
        
		if(!isset($_GET["page"]))
	        $url=$url."&page=0";

		$page_n = 10*floor($page/10);
		$upperlimit = floor(($s->GetCount()-1)/10);
		
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