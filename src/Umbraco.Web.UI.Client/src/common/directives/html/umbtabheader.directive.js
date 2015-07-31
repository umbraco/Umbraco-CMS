/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTabHeader 
* @restrict E
* @function
* @description 
* The header on an editor that contains tabs
**/
angular.module("umbraco.directives").directive('umbTabHeader', function($parse, $timeout) {
    return {
        require: "?^umbTabs",
        restrict: 'E',
        replace: true,
        transclude: 'true',
        templateUrl: 'views/directives/umb-tab-header.html',
        link: function(scope, iElement, iAttrs, tabsCtrl) {

            scope.showTabs = false;
            scope.activeTabId = null;
            scope.tabs = [];

            if (tabsCtrl) {
                tabsCtrl.onTabCollectionChanged(function (tabs) {
                    scope.tabs = tabs;
                    scope.showTabs = scope.tabs.length > 0;
                });

                tabsCtrl.onActiveTabChanged(function (tabId) {
                    scope.activeTabId = tabId;
                });

                scope.changeTab = function (tabId) {
                    tabsCtrl.setActiveTab(tabId);
                };

                //TODO: We'll need to destroy this I'm assuming!
                iElement.find('.nav-pills, .nav-tabs').tabdrop();
            }
           
        }
    };
});