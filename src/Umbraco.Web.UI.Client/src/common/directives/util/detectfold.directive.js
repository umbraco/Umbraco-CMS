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
			link: function (scope, el, attrs) {

			    var state = false;
			    var parent = $(".umb-panel-body");
			    var winHeight = $(window).height();
			    var calculate = function() {
			        if (el && el.is(":visible") && !el.hasClass("umb-bottom-bar")) {
			            //var parent = el.parent();
			            var hasOverflow = parent.innerHeight() < parent[0].scrollHeight;
			            //var belowFold = (el.offset().top + el.height()) > winHeight;
			            if (hasOverflow) {
			                el.addClass("umb-bottom-bar");
			            }
			        }
			    };

			    var resizeCallback = function(size) {
			        winHeight = size.height;
			        el.removeClass("umb-bottom-bar");
			        //state = false;
			        calculate();
			    };

			    windowResizeListener.register(resizeCallback);

			    
                //Required for backwards compat for bootstrap tabs
			    $('a[data-toggle="tab"]').on('shown', calculate);

                //ensure to unregister
			    scope.$on('$destroy', function() {
			        windowResizeListener.unregister(resizeCallback);
			        $('a[data-toggle="tab"]').off('shown', calculate);
			    });
			}
		};
	});