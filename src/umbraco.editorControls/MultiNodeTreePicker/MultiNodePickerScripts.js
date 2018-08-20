(function ($) {

    //jquery plugin for our tree picker
    $.fn.MultiNodeTreePicker = function (ctlId, maxItems, tooltipAjaxUrl, showTooltip, showThumbnail, umbPath, treeType) {

        /* internal properties */
        var $tree = $(this);
        var ctl = $("#" + ctlId);
        var hiddenField = ctl.find("input[type='hidden']");
        var rightCol = ctl.find(".right");
        var tooltip = null; //the tooltip object (will be defined when it is shown)
        var throbber = null;
        var collapseNode = null; //this is used to track collapsing of tree nodes due to the issue of the nodeClicked event being fired on tree collapse

        //store a reference to the hidden field in the right columns data collection
        rightCol.data("hiddenField", hiddenField);

        function getPosition(trigger, tip, conf) {
            // get origin top/left position 
            var top = trigger.offset().top,
                left = trigger.offset().left,
                pos = conf.position[0];

            top -= tip.outerHeight() - conf.offset[0];
            left += trigger.outerWidth() + conf.offset[1];

            // adjust Y		
            var height = tip.outerHeight() + trigger.outerHeight();
            if (pos == 'center') { top += height / 2; }
            if (pos == 'bottom') { top += height; }


            // adjust X
            pos = conf.position[1];
            var width = tip.outerWidth() + trigger.outerWidth();
            if (pos == 'center') { left -= width / 2; }
            if (pos == 'left') { left -= width; }

            return { top: top, left: left };
        }

        //used to create the tooltip
        var tooltipOptions = {
            tip: "#MNTPTooltip",
            effect: "fade",
            predelay: 0,
            position: 'center left',
            relative: true,
            offset: [30, 0],
            onShow: function () {
                //get the id of the item being queried
                var id = this.getTrigger().next().find("li").attr("rel");
                $.ajax({
                    type: "POST",
                    data: "{\"id\": " + id + "}",
                    dataType: "json",
                    url: tooltipAjaxUrl,
                    contentType: "application/json; charset=UTF-8",
                    success: function (data) {
                        var newLocation = (treeType == "content" ? umbPath + "/editContent.aspx?id=" : umbPath + "/editMedia.aspx?id=") + data.d.Id;
                        var h = $("<a href='" + newLocation + "'>[edit]</a><h5>ID: " + data.d.Id + "</h5><p><b>Path:</b> " + data.d.Path + "</p><p><i>" + data.d.PathAsNames + "</i></p>");
                        h.click(function () {

                            if (!confirm("Are you sure you want to navigate away from this page?\n\nYou may have unsaved changes.\n\nPress OK to continue or Cancel to stay on the current page.")) {
                                return false;
                            }

                            //this is a VERY dodgy work around for deep linking between sections and pages
                            var iframe = UmbClientMgr.mainWindow().jQuery("#deepLinkScriptFrame");
                            if (iframe.length == 0) {
                                var html = "<html><head><script type='text/javascript'>"
                                            + "this.window.top.delayedNavigate = function(url, app) { "
                                            + "  if (UmbClientMgr.historyManager().getCurrent() == app) {"
                                            + "    UmbClientMgr.contentFrame(url);"
                                            + "  }"
                                            + "  else {"
                                            + "    var origContentFrameFunc = UmbClientMgr.contentFrame;"
                                            + "    var newContentFrameFunc = function (location) {"
                                            + "       UmbClientMgr.contentFrame = origContentFrameFunc;"
                                            + "       origContentFrameFunc.call(this, url);"
                                            + "    };"
                                            + "    UmbClientMgr.contentFrame = newContentFrameFunc;"
                                            + "    UmbClientMgr.mainTree()._loadedApps['tree_' + app] = null;"
                                            + "    UmbClientMgr.mainTree().setActiveTreeType(app);"
                                            + "    UmbClientMgr.mainWindow().location.hash = '#' + app   ; "
                                            + "  }"
                                            + "};"
                                            + "</script></head><body></body></html>";
                                iframe = UmbClientMgr.mainWindow().jQuery("<iframe id='deepLinkScriptFrame'>")
                                            .append(html)
                                            .hide()
                                            .css("width", "0px")
                                            .css("height", "0px");
                                UmbClientMgr.mainWindow().jQuery("body").append(iframe);
                            }

                            UmbClientMgr.mainWindow().delayedNavigate(newLocation, treeType);

                            return false;
                        });
                        throbber.hide().next().html("").append(h).show();
                    },
                    error: function (data) {
                        alert("Error!" + data.d.Message);
                    }
                });
            },
            onBeforeShow: function (ev, pos) {
                tooltip = this.getTip();
                //move the tooltip just before the trigger so that it's relatively placed
                this.getTrigger().before(tooltip);
                throbber = tooltip.find(".throbber");
                throbber.show().next().hide();
            },
            events: {
                def: 'click, mouseleave'
            }
        };

        function StorePickedNodes(hiddenField, rightCol) {
            if (!hiddenField || !rightCol)
                return;

            var val = "";
            rightCol.find(".item ul.rightNode li").each(function () {
                val += $(this).attr("rel") + ",";
            });
            if (val != "") val = val.substr(0, val.length - 1);
            hiddenField.val(val);
        };

        function doHighlight(node) {
            var div = node.find('a').find("div:first");
            var origBorder = div.css("border");
            div.css("border", "1px solid #FBC2C4").css("background-color", "#FBE3E4").css("color", "#8a1f11");
            setTimeout(function () { div.attr("style", ""); }, 500);
        }

        //handler for the node clicking event
        function nodeClickHandler(e, node) {
            //this is a dodgy hack due to a bug in the umb tree that fires the click event 
            //twice: http://umbraco.codeplex.com/workitem/29194
            if ($(node).is(":hidden")) {
                collapseNode = node;
                return;
            }
            else {
                //if the collapseNode flag is null, then we're ok
                if (!collapseNode) {
                    if (!$(node).hasClass("uc-treenode-noclick")) {
                        AddToRight(node);
                    }
                    else {
                        doHighlight($(node));
                    }
                }
                else {
                    //reset the flag but still do nothing
                    collapseNode = null;
                }
            }
        }

        //handler for when the tree is synced
        function treeSyncEventHandler() {
            //re-add a handler to the tree's nodeClicked event
            $tree.UmbracoTreeAPI().addEventHandler("nodeClicked", nodeClickHandler);
        }

        //syncs the tree with the item selected on the right
        function SyncItems(node) {
            //first remove the nodeClick event handler as this will fire when the tree is synced
            $tree.UmbracoTreeAPI().removeEventHandler("nodeClicked", nodeClickHandler);

            //for some reason node syncing doesn't work until a node is selected, so lets just
            //select the root node, then continue to sync the tree.
            $tree.UmbracoTreeAPI().selectNode($tree.find("li:first"));

            //the path will be available to nodes rendered when the page is rendered so we can do a full sync tree.
            //only the node id will be available for nodes that have been newly selected but this is ok since the
            //nodes are already loaded so the system will be able to sync them.
            var nodeId = node.attr("rel");
            var path = node.attr("umb:nodedata");
            if (!path) path = nodeId;

            $tree.UmbracoTreeAPI()
                .syncTree(path.toString());
        }

        //does the adding of a node to the right hand column
        //If "test" is true the node is not added. Instead true is returned if it could have been added
        function AddToRight(node, test) {

            var $node = $(node);

            if ($node.hasClass("uc-treenode-noclick")) {
                doHighlight($node);
                return "Item not allowed here";
            }

            //get the node id of the node selected
            var nodeId = $node.attr("id");

            //first, check if we've reached the max
            if (maxItems >= 0 && rightCol.find("li").length >= maxItems) {
                doHighlight($node);
                return "No more items can be added";
            }

            //check if node id already exists in the right panel, also check if it the root node
            //since this should not be selectable
            if (nodeId <= 0 || $tree.find("li:first").attr("id") == nodeId || (rightCol.find("li[rel='" + nodeId + "']").length > 0)) {
                doHighlight($node);
                return nodeId < 0 ? "Item not allowed here" : "Item already added";
            }

            if (test) {
                return true;
            }

            //create a copy of the node clicked on the tree
            var jNode = $node.clone().find("a:first")
            //remove un-needed attributes
            jNode.removeAttr("href")
                .removeAttr("umb:nodedata")
                .attr("href", "#")
                .attr("title", "Sync tree");

            //build a DOM object to put in the right panel
            var inserted = $("<div class='item'><a href='javascript:void(0);' class='info'></a>" +
                            "<div class='inner'><ul class='rightNode'>" +
                            "<li rel='" + nodeId + "' class='closed'>" +
                            "</li></ul><a class='close' title='Remove' href='javascript:void(0);'></a></div></div>")
                                .hide()
                                .appendTo(rightCol) //add the full div to the right col
                                .find(".closed") //get the li element
                                .append(jNode) //append the anchor link
                                .closest(".item"); //get the item div                                

            if (showTooltip) {
                inserted.find(".info").tooltip(tooltipOptions) //add the tooltop
            }
            else {
                //remove the tooltip
                inserted.find(".info").remove();
            }

            //add the image preview if we need to
            if (showThumbnail) {
                //set the item height to 50px and the width of the inner to 224px
                inserted.css("height", "50px").find(".inner").css("width", "224px");
                var imgViewer = $("<div class='imageViewer'></div>").prependTo(inserted);
                //create the image viewer object, get the API of it and update the image
                imgViewer.UmbracoImageViewer({
                    umbPath: umbPath,
                    style: "Basic",
                    linkTarget: "_blank"
                })
                .UmbracoImageViewerAPI()
                .updateImage(nodeId, function (args) {
                    if (imgViewer.find("a").attr("href") == "#") {
                        imgViewer.find("img").attr("src", umbPath + "/images/blank.png");
                    }
                });
            }

            inserted.show();

            //now update the hidden field with the
            //node selection
            StorePickedNodes(hiddenField, rightCol);
        }

        //remove all trashed nodes        
        rightCol.find("li[rel='trashed']").closest(".item").remove();

        //click handlers for the removal of items
        $(".item a.close", rightCol).on("click", function () {
            $(this).closest(".item").remove();
            StorePickedNodes(hiddenField, rightCol);
        });
        //create click handlers to the right hand items
        $(".item ul li a", rightCol).on("click", function () {
            SyncItems($(this).parent());
        });

        //add a handler to the tree's nodeClicked event
        $tree.UmbracoTreeAPI().addEventHandler("nodeClicked", nodeClickHandler);
        $tree.UmbracoTreeAPI().addEventHandler("syncNotFound", treeSyncEventHandler);
        $tree.UmbracoTreeAPI().addEventHandler("syncFound", treeSyncEventHandler);

        //create a sortable, drag/drop list and
        //initialize the right panel with previously
        //saved data.
        rightCol.sortable({
            stop: function (event, ui) { StorePickedNodes($(this).data("hiddenField"), $(this)); },
            start: function (event, ui) { if (tooltip) tooltip.hide(); }, //hide the tooltip when sorting
            handle: '.inner'
        });

        //add the tooltips        
        rightCol.find("a.info").tooltip(tooltipOptions);

        //Register drag and drop action
        //PROOF OF CONCEPT: The user can probably also drop stuff that shouldn't be dropped (e.g., StartNodeID is not considered). The filter function can take care of that
        if (typeof (UmbDragDrop) != "undefined") {
            UmbDragDrop.register(
            //Container to highlight
            rightCol,
            //Drop action
            function (info) {
                AddToRight(info.drag_node);
            },
            //Filter
            function (info) {
                var dropMessage = AddToRight(info.drag_node, true);
                return { canDrop: dropMessage === true, message: dropMessage === true ? "" : dropMessage };
            });
        }
    }

})(jQuery);