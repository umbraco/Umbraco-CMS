var Our = Our || {};
Our.Umbraco = Our.Umbraco || {};
Our.Umbraco.uGoLive = Our.Umbraco.uGoLive || {};

(function ($) {

    Our.Umbraco.uGoLive.Dashboard = (function() {

        var checks = [];
        var currentCheckIndex = -1;
		var opts = {
			basePath: "/base",
			umbracoPath: "/umbraco"
		};
        
        function performNextCheck() {
			
			// Get the current check id
            var checkId = checks[currentCheckIndex];
			
			// Trigger check
            performCheck(checkId, function(data) {
				
				// Trigger next check, or finish
                if(currentCheckIndex + 1 == checks.length) {
					
                    // Re-enable the "Run All Checks" button
                    var $btn = $("#btnRunChecks");
                    $btn.text("Re-Run All Checks");
                    $btn.removeClass("disabled");
					
                } else {
					
                    // Run the next check
                    currentCheckIndex++;
                    performNextCheck();
					
                }
            });
        }
        
        function performCheck(checkId, callBack) {
			
			// Get references
			var $checkEl = $("span.status[data-check-id=" + checkId + "]");
			var $checkButton = $("a.check[data-check-id=" + checkId + "]");
			var $rectifyButton = $("a.rectify[data-check-id=" + checkId + "][data-check-can-rectify='true']");
			
			// Disable buttons
			$checkButton.addClass("disabled");
			$rectifyButton.addClass("disabled");
			
			// Display throbber
            $checkEl.html("<img src='" + opts.umbracoPath + "/plugins/uGoLive/throbber.gif' alt='Checking...' /> Checking...");
            
			// Perform check
			$.getJSON(opts.basePath + '/uGoLive/Check/'+ checkId +'.aspx', function(data) {

				// Remove throbber
                $checkEl.empty();
                
				// Display icon & enable / disable rectify button
                switch(data.Status.Value) {
                    case "Passed":
                        $checkEl.append("<img src='" + opts.umbracoPath + "/plugins/uGoLive/tick.png' alt='Passed' />");
                        $rectifyButton.addClass("disabled");
                        break;
                    case "Indeterminate":
                        $checkEl.append("<img src='" + opts.umbracoPath + "/plugins/uGoLive/error.png' alt='Indeterminate' />");
                        $rectifyButton.removeClass("disabled");
                        break;
                    case "Failed":
                        $checkEl.append("<img src='" + opts.umbracoPath + "/plugins/uGoLive/cross.png' alt='Failed' />");
                        $rectifyButton.removeClass("disabled");
                        break;
                }
                
				// Display message
                if($.trim(data.Message) != "")
                    $checkEl.append(data.Message);

				// Re-enable check button
				$checkButton.removeClass("disabled");

				// Execute callback
                if(callBack != undefined)
                    callBack(data);
            });
        }
        
        function performRectify(checkId, callBack) {
			
			// Get references
			var $checkEl = $("span.status[data-check-id=" + checkId + "]");
			var $checkButton = $("a.check[data-check-id=" + checkId + "]");
			var $rectifyButton = $("a.rectify[data-check-id=" + checkId + "][data-check-can-rectify='true']");

			// Disable buttons
			$checkButton.addClass("disabled");
			$rectifyButton.addClass("disabled");
			
			// Display throbber
            $checkEl.html("<img src='" + opts.umbracoPath + "/plugins/uGoLive/throbber.gif' alt='Rectifying...' /> Rectifying...");
            
			// Perform rectify
			$.getJSON(opts.basePath + '/uGoLive/Rectify/'+ checkId +'.aspx', function(data) {

				// Remove throbber
                $checkEl.empty();
                
				// Display icon & enable / disable rectify button
                switch(data.Status.Value) {
                    case "Success":
                        $checkEl.append("<img src='" + opts.umbracoPath + "/plugins/uGoLive/tick.png' alt='Passed' />");
                        $rectifyButton.addClass("disabled");
                        break;
                    case "Failed":
                        $checkEl.append("<img src='" + opts.umbracoPath + "/plugins/uGoLive/cross.png' alt='Failed' />");
                        $rectifyButton.removeClass("disabled");
                        break;
                }
                
				// Display message
                if($.trim(data.Message) != "")
                    $checkEl.append(data.Message);

				// Re-enable check button
				$checkButton.removeClass("disabled");

				// Execute callback
                if(callBack != undefined)
                    callBack(data);
            });
        }
        
        return {
            
            init: function (o) {
				
				// Merge options
				opts = $.extend(opts, o);

                // Parse all checks
                $("span.status").each(function (idx, el) {
                    checks.push($(el).attr("data-check-id"));
                });
                
                // Hookup run all check button
                $("#btnRunChecks").click(function(e) {

                    e.preventDefault();

                    var $this = $(this);
                    
                    if(!$this.hasClass("disabled")) {
                        
                        // Clear out previous checks
                        $("span.status").empty();
						
						// Disable check/rectify buttons
                        $("a.check").addClass("disabled");
                        $("a.rectify").addClass("disabled");

                        // Update run checks button
                        $this.text("Running checks...");
                        $this.addClass("disabled");

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
                        performCheck($this.attr("data-check-id"));
                    }
                });
                
                // Hookup individual rectify buttons
                $("a.rectify").click(function(e) {

                    e.preventDefault();

                    var $this = $(this);
                    
                    if(!$this.hasClass("disabled")) {
                        performRectify($this.attr("data-check-id")); 
                    }
                });
            }
            
        };
        
    })();
    
})(jQuery)