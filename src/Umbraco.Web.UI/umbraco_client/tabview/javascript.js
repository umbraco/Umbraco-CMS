/// <reference path="../Application/NamespaceManager.js" />

Umbraco.Sys.registerNamespace('Umbraco.Controls');

Umbraco.Controls.TabView = (function () {
    var onChangeEvents = [];

    var obj = {
        setActiveTab: function (tabviewid, tabid, tabs) {
            for (var i = 0; i < tabs.length; i++) {
                jQuery("#" + tabs[i]).attr("class", "tabOff");
                jQuery("#" + tabs[i] + "layer").hide();
            }

            var activeTab = jQuery("#" + tabid).attr("class", "tabOn");
            jQuery("#" + tabid + "layer").show();
            jQuery("#" + tabviewid + '_activetab').val(tabid);

            // show first tinymce toolbar
            jQuery(".tinymceMenuBar").hide();
            jQuery(document).ready(function () {
                jQuery("#" + tabid + "layer .tinymceMenuBar:first").show();
            });
            for (var i = 0; i < onChangeEvents.length; i++) {
                var fn = onChangeEvents[i];
                fn.apply(activeTab, [tabviewid, tabid, tabid]);
            }
        },

        onActiveTabChange: function (fn) {
            onChangeEvents.push(fn);
        },

        resizeTabViewTo: function (TabPageArr, TabViewName, tvHeight, tvWidth) {
            if (!tvHeight) {
                tvHeight = jQuery(window).height();   //getViewportHeight();
                if (document.location) {
                    tvHeight = tvHeight - 10;
                }
            }
            if (!tvWidth) {
                tvWidth = jQuery(window).width(); // getViewportWidth();
                if (document.location) {
                    tvWidth = tvWidth - 10;
                }
            }

            var tabviewHeight = tvHeight - 12;

            jQuery("#" + TabViewName).width(tvWidth);
            jQuery("#" + TabViewName).height(tabviewHeight);


            for (i = 0; i < TabPageArr.length; i++) {
                scrollwidth = tvWidth - 30;
                jQuery("#" + TabPageArr[i] + "layer_contentlayer").height((tabviewHeight - 67));

                //document.getElementById(TabPageArr[i] +"layer_contentlayer").style.border = "2px solid #fff";

                jQuery("#" + TabPageArr[i] + "layer_contentlayer").width((tvWidth - 2));
                jQuery("#" + TabPageArr[i] + "layer_menu").width((scrollwidth));
                jQuery("#" + TabPageArr[i] + "layer_menu_slh").width((scrollwidth));
            }
        },

        tabSwitch: function (direction) {
            var preFix = "TabView1";
            var currentTab = jQuery("#" + preFix + "_activetab").val();

            currentTab = currentTab.substr(preFix.length + 4, currentTab.length - preFix.length - 4);
            var nextTab = Math.round(currentTab) + direction;

            if (nextTab < 10)
                nextTab = "0" + nextTab;

            // Try to grab the next one!
            if (nextTab != "00") {
                if (jQuery("#" + preFix + '_tab' + nextTab) != null) {
                    setActiveTab(preFix, preFix + '_tab' + nextTab, eval(preFix + '_tabs'));
                }
            }
        }
    };

    return obj;
})();

function setActiveTab(tabviewid, tabid, tabs) {
    Umbraco.Controls.TabView.setActiveTab(tabviewid, tabid, tabs);
}

function resizeTabView(TabPageArr, TabViewName) {
    Umbraco.Controls.TabView.resizeTabViewTo(TabPageArr, TabViewName);
}

function resizeTabViewTo(TabPageArr, TabViewName, tvHeight, tvWidth) {
    Umbraco.Controls.TabView.resizeTabViewTo(TabPageArr, TabViewName, tvHeight, tvWidth);
}

function tabSwitch(direction) {
    Umbraco.Controls.TabView.tabSwitch(direction);
}