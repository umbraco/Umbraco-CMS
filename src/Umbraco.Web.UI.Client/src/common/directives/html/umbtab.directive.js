/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTab 
* @restrict E
**/
angular.module("umbraco.directives")
.directive('umbTab', function ($parse) {
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
		        if (scope.tabId === String(tabId)) {
		            elem.show();
		        }
		        else {
		            elem.hide();
		        }
		    }

		    //need to make this optional for backwards compat since before we used to
		    // use bootstrap tabs and now we use our own faster implementation which
            // gives us far more control but will still support the old way.
		    if (tabsCtrl != null) {

                tabsCtrl.onActiveTabChanged(function (tabId) {
                    toggleVisibility(tabId);
                });

                toggleVisibility(tabsCtrl.getActiveTab());		        
		    }
        }
    };
});