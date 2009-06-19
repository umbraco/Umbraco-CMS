
function setActiveTab(tabviewid,tabid,tabs) {
    for (var i = 0; i < tabs.length; i++) {
        jQuery("#" + tabs[i]).attr("class", "tabOff");
        jQuery("#" + tabs[i] + "layer").hide(); 
       }

       jQuery("#" + tabid).attr("class", "tabOn");
       jQuery("#" + tabid + "layer").show();
       jQuery("#" + tabviewid + '_activetab').val(tabid);

	   // show first tinymce toolbar
       jQuery(".tinymceMenuBar").hide();
       jQuery(document).ready(function() {
           jQuery("#" + tabid + "layer .tinymceMenuBar:first").show();
       });
}

function resizeTabView(TabPageArr, TabViewName) {
    var clientHeight = jQuery(window).height();   //getViewportHeight();
    var clientWidth = jQuery(window).width(); // getViewportWidth();
    
	if(top.document.location == document.location)
        resizeTabViewTo(TabPageArr, TabViewName, clientHeight - 10, clientWidth - 15)
	else
	    resizeTabViewTo(TabPageArr, TabViewName, clientHeight, clientWidth)
}

function resizeTabViewTo(TabPageArr, TabViewName, tvHeight, tvWidth) {
    var tabviewHeight = tvHeight - 12;
    
	jQuery("#" + TabViewName).width(tvWidth);
	jQuery("#" + TabViewName).height(tabviewHeight);
	
				
	for (i=0;i<TabPageArr.length;i++) {
		scrollwidth = tvWidth - 30;
		jQuery("#" + TabPageArr[i] + "layer_contentlayer").height( (tabviewHeight - 67) );
		
		//document.getElementById(TabPageArr[i] +"layer_contentlayer").style.border = "2px solid #fff";

		jQuery("#" + TabPageArr[i] + "layer_contentlayer").width( (tvWidth - 2) );
		jQuery("#" + TabPageArr[i] + "layer_menu").width( (scrollwidth) );
		jQuery("#" + TabPageArr[i] + "layer_menu_slh").width( (scrollwidth) );
	}
}

function tabSwitch(direction) {
	var preFix = "TabView1";
	var currentTab = jQuery("#" + preFix +"_activetab").val();
	
	currentTab = currentTab.substr(preFix.length+4,currentTab.length-preFix.length-4);
	var nextTab = Math.round(currentTab)+direction;
	
	if (nextTab < 10)
		nextTab = "0" + nextTab;
	
	// Try to grab the next one!
	if (nextTab != "00") {
		if (jQuery("#" + preFix+'_tab' + nextTab) != null) {
			setActiveTab(preFix,preFix + '_tab' + nextTab,eval(preFix+'_tabs'));
		}
	}
}