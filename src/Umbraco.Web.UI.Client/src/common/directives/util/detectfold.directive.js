/**
* @ngdoc directive
* @name umbraco.directives.directive:umbPanel
* @description This is used for the editor buttons to ensure they are displayed correctly if the horizontal overflow of the editor
 * exceeds the height of the window
**/
angular.module("umbraco.directives.html")
	.directive('detectFold', function ($timeout, $log, windowResizeListener) {
	    return {
            require: "^?umbTabs",
			restrict: 'A',
			link: function (scope, el, attrs, tabsCtrl) {

			    var firstRun = false;
			    var parent = $(".umb-panel-body");
			    var winHeight = $(window).height();
			    var calculate = function () {
			        if (el && el.is(":visible") && !el.hasClass("umb-bottom-bar")) {

			            //now that the element is visible, set the flag in a couple of seconds, 
			            // this will ensure that loading time of a current tab get's completed and that
			            // we eventually stop watching to save on CPU time
			            $timeout(function() {
			                firstRun = true;
			            }, 4000);

			            //var parent = el.parent();
			            var hasOverflow = parent.innerHeight() < parent[0].scrollHeight;
			            //var belowFold = (el.offset().top + el.height()) > winHeight;
			            if (hasOverflow) {
			                el.addClass("umb-bottom-bar");

			                //I wish we didn't have to put this logic here but unfortunately we 
			                // do. This needs to calculate the left offest to place the bottom bar
			                // depending on if the left column splitter has been moved by the user
                            // (based on the nav-resize directive)
			                var wrapper = $("#mainwrapper");
			                var contentPanel = $("#leftcolumn").next();
			                var contentPanelLeftPx = contentPanel.css("left");

			                el.css({ left: contentPanelLeftPx });
			            }
			        }
			        return firstRun;
			    };

			    var resizeCallback = function(size) {
			        winHeight = size.height;
			        el.removeClass("umb-bottom-bar");
			        calculate();
			    };

			    windowResizeListener.register(resizeCallback);

			    //Only execute the watcher if this tab is the active (first) tab on load, otherwise there's no reason to execute
			    // the watcher since it will be recalculated when the tab changes!
                if (el.closest(".umb-tab-pane").index() === 0) {
                    //run a watcher to ensure that the calculation occurs until it's firstRun but ensure
                    // the calculations are throttled to save a bit of CPU
                    var listener = scope.$watch(_.throttle(calculate, 1000), function (newVal, oldVal) {
                        if (newVal !== oldVal) {
                            listener();
                        }
                    });
                }

			    //listen for tab changes
                if (tabsCtrl != null) {
                    tabsCtrl.onTabShown(function (args) {
                        calculate();
                    });
                }
			    
			    //ensure to unregister
			    scope.$on('$destroy', function() {
			        windowResizeListener.unregister(resizeCallback);			       
			    });
			}
		};
	});