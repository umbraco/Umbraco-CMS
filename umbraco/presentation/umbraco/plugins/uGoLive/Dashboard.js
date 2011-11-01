var Our = Our || {};
Our.Umbraco = Our.Umbraco || {};
Our.Umbraco.uGoLive = Our.Umbraco.uGoLive || {};

(function ($) {

    Our.Umbraco.uGoLive.Dashboard = (function() {

        var checks = [];
        var currentCheckIndex = -1;
		var basePath = "/base";
		var umbracoPath = "/umbraco";
        
        function performNextCheck() {
            var checkId = checks[currentCheckIndex];
            performCheck(checkId, function(data) {
                if(currentCheckIndex + 1 == checks.length) {
                    // Re-enable the "Run All Checks" button
                    var $btn = $("#btnRunChecks");
                    $btn.text("Re-Run All Checks");
                    $btn.removeClass("disabled");
                    $("a.check").removeAttr("disabled");
                } else {
                    // Run the next check
                    currentCheckIndex++;
                    performNextCheck();
                }
            });
        }
        
        function performCheck(checkId, callBack) {
            $("span.status[data-check-id=" + checkId + "]").html("<img src='" + umbracoPath + "/plugins/uGoLive/throbber.gif' alt='Checking...' /> Checking...");
            $.getJSON(basePath + '/uGoLive/Check/'+ checkId +'.aspx', function(data) {

                var $checkEl = $("span.status[data-check-id=" + checkId + "]");

                $checkEl.empty();
                
                switch(data.Status.Value) {
                    case "Passed":
                        $checkEl.append("<img src='" + umbracoPath + "/plugins/uGoLive/tick.png' alt='Passed' />");
                        $("a.rectify[data-check-id=" + checkId + "][data-check-can-rectify='true']").addClass("disabled");
                        break;
                    case "Indeterminate":
                        $checkEl.append("<img src='" + umbracoPath + "/plugins/uGoLive/error.png' alt='Indeterminate' />");
                        $("a.rectify[data-check-id=" + checkId + "][data-check-can-rectify='true']").removeClass("disabled");
                        break;
                    case "Failed":
                        $checkEl.append("<img src='" + umbracoPath + "/plugins/uGoLive/cross.png' alt='Failed' />");
                        $("a.rectify[data-check-id=" + checkId + "][data-check-can-rectify='true']").removeClass("disabled");
                        break;
                }
                
                if($.trim(data.Message) != "")
                    $checkEl.append(data.Message);

                if(callBack != undefined)
                    callBack(data);
            });
        }
        
        function performRectify(checkId, callBack) {
            $("span.status[data-check-id=" + checkId + "]").html("<img src='" + umbracoPath + "/plugins/uGoLive/throbber.gif' alt='Rectifying...' /> Rectifying...");
            $.getJSON(basePath + '/uGoLive/Rectify/'+ checkId +'.aspx', function(data) {

                var $checkEl = $("span.status[data-check-id=" + checkId + "]");

                $checkEl.empty();
                
                switch(data.Status.Value) {
                    case "Success":
                        $checkEl.append("<img src='" + umbracoPath + "/plugins/uGoLive/tick.png' alt='Passed' />");
                        $("a.rectify[data-check-id=" + checkId + "][data-check-can-rectify='true']").addClass("disabled");
                        break;
                    case "Failed":
                        $checkEl.append("<img src='" + umbracoPath + "/plugins/uGoLive/cross.png' alt='Failed' />");
                        $("a.rectify[data-check-id=" + checkId + "][data-check-can-rectify='true']").removeClass("disabled");
                        break;
                }
                
                if($.trim(data.Message) != "")
                    $checkEl.append(data.Message);

                if(callBack != undefined)
                    callBack(data);
            });
        }
        
        return {
            
            init: function (o) {
				
				// Set the paths
				if (typeof(o.umbracoPath) != 'undefined')
					umbracoPath = o.umbracoPath;
				if (typeof(o.basePath) != 'undefined')
					basePath = o.basePath;

                // Parse all checks
                $("span.status").each(function (idx, el)
                {
                    checks.push($(el).attr("data-check-id"));
                });
                
                // Hookup run all check button
                $("#btnRunChecks").click(function(e) {

                    e.preventDefault();

                    var $this = $(this);
                    
                    if(!$this.hasClass("disabled")) {
                        
                        // Clear out previous checks
                        $("span.status").empty();

                        // Update button
                        $this.text("Running checks...");
                        $this.addClass("disabled");
                        $("a.check").attr("disabled", "disabled");

                        // Start checks
                        currentCheckIndex = 0;
                        performNextCheck();
                        
                    }
                });
                
                // Hookup individual check buttons
                $("a.check").click(function(e) {

                    e.preventDefault();

                    var $this = $(this);
                    
                    if(!$this.hasClass("disabled")) {
                        
                        $this.addClass("disabled");
                        var checkId = $this.attr("data-check-id");
                        performCheck(checkId, function(data) {
                            $this.removeClass("disabled");
                        });
                        
                    }
                });
                
                // Hookup individual rectify buttons
                $("a.rectify").click(function(e) {

                    e.preventDefault();

                    var $this = $(this);
                    
                    if(!$this.hasClass("disabled")) {
                        
                        $this.addClass("disabled");
                        var checkId = $this.attr("data-check-id");
                        performRectify(checkId, function(data) {
                            //$this.removeAttr("disabled");
                        });
                        
                    }
                });
            }
            
        };
        
    })();
    
})(jQuery)