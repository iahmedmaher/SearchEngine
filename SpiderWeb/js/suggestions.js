var currentRequest = null;    
var currentlistitem = 0;
var currentlistcount = 0;

$(document).ready(function() {

	$('#query-text').on('input propertychange paste', function() {
		var txt = $("#query-text").val();
		if(txt!="")
		{
			$(".autocomplete-suggestions").css("display","");
			currentRequest = $.ajax({
				url: "ajax/suggestions.php",
				data: {
					q: txt
				},
				
				beforeSend : function()    {           
					if(currentRequest != null) {
						currentRequest.abort();
					}
				},
				
				success: function(dataAjax) {
					$(".autocomplete-suggestions").html("");
					var jsonData = JSON.parse(dataAjax);
					var regEx = new RegExp(txt, "ig");
					for (var i = 0; i < jsonData.length; i++) {
						$(".autocomplete-suggestions").append('<div class="autocomplete-suggestion" id="suggestion-item'+(i+1)+'">'+jsonData[i].replace(regEx,"<b>" + jsonData[i].match(regEx) + "</b>")+'</div>');
					}
					count = jsonData.length;
					currentlistitem = 0;
					setup();
				}
			})
		}
		else
		{
			$(".autocomplete-suggestions").html("");
			$(".autocomplete-suggestions").css("display","none");
		}
	});
	
	$('#query-text').keydown(function(e) {
    switch(e.which) {
        case 38: // up
			if(currentlistitem==0)
				break;
			else if(currentlistitem!=1)
			{
				$("#suggestion-item"+(currentlistitem-1)).addClass("autocomplete-selected");
				$("#query-text").val($("#suggestion-item"+(currentlistitem-1)).text());
			}
			$("#suggestion-item"+currentlistitem).removeClass("autocomplete-selected");
			currentlistitem--;
        break;
		
        case 40: // down
			if(currentlistitem==count+1)
				break;
			else if(currentlistitem!=count)
			{
				$("#suggestion-item"+(currentlistitem+1)).addClass("autocomplete-selected");
				$("#query-text").val($("#suggestion-item"+(currentlistitem+1)).text());
			}
			$("#suggestion-item"+currentlistitem).removeClass("autocomplete-selected");
			currentlistitem++;
        break;

        default: return; // exit this handler for other keys
    }
    e.preventDefault(); // prevent the default action (scroll / move caret)
});
	
	
});

function setup()
{
	$(".autocomplete-suggestion").click(function(){
		$("#query-text").val($(this).text());
		$(".autocomplete-suggestions").css("display","none");
		$("#search-form").submit();		
	});
	
	$(".autocomplete-suggestion").mouseenter(function(){
		$(this).addClass("autocomplete-selected");
	});

	$(".autocomplete-suggestion").mouseleave(function(){
		$(this).removeClass("autocomplete-selected");
	});
}