/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTab 
* @restrict E
**/
angular.module("umbraco.directives")
.directive('umbTab', function ($parse, $timeout) {
    return {
        require: "?^umbTabs",
		restrict: 'E',
		replace: true,
		scope: {
		    id : "@",
            tabId : "@rel"
        },
        transclude: 'true',
		templateUrl: 'views/directives/umb-tab.html',
		link: function(scope, elem, attrs, tabsCtrl) {
            
		    function toggleVisibility(tabId) {
                //default if there are no tabs
		        if (tabId === null) {
		            elem.addClass("active");
		        }
		        else {
		            if (scope.tabId === String(tabId)) {
		                elem.addClass("active");
		            }
		            else {
		                elem.removeClass("active");
		            }
		        }
		    }

		    //need to make this optional for backwards compat since before we used to
		    // use bootstrap tabs and now we use our own better implementation which
            // gives us far more control but will still support the old way.
		    if (tabsCtrl) {

                tabsCtrl.onActiveTabChanged(function (tabId) {
                    toggleVisibility(tabId);
                });

                toggleVisibility(tabsCtrl.getActiveTab());

		    }
        }
    };
});