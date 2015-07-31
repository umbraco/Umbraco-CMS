/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTabView 
* @restrict E
**/
angular.module("umbraco.directives")
.directive('umbTabView', function($timeout, $log){
    return {
        require: "?^umbTabs",
		restrict: 'E',
		replace: true,
		transclude: 'true',
		templateUrl: 'views/directives/umb-tab-view.html',
		compile: function () {
		    return {
		        pre: function (scope, iElement, iAttrs, tabsCtrl) {
		            //if we have our custom tab directive it means we are not using bootstrap
		            // tabs, however if there isn't a directive we'll continue using bootsrap tabs
		            // this is done by adding 'tab-content' class for bootstrap or 'umb-tab-content'
		            // for our custom tabs.

		            //We also MUST do this on pre-linking because the tab-content class needs to be there
                    // before it hits the DOM so that bootstrap can do whatever it is that it does.

		            if (tabsCtrl) {
		                iElement.children("div:first").addClass("umb-tab-content");
		            }
		            else {
		                iElement.children("div:first").addClass("tab-content");
		            }
		        }
		    };
		}
	};
});