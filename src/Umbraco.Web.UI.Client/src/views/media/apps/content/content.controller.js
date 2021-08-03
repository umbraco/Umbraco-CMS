(function () {
    "use strict";

    function MediaAppContentController($scope, $filter, contentEditingHelper) {

        var vm = this;

        vm.tabs = [];
        vm.activeTabAlias = null;

        vm.setActiveTab = setActiveTab;

        $scope.$watchCollection('content.tabs', () => {

            // Add parentAlias property to all groups aka. tabs.
            $scope.content.tabs.forEach((group) => {
                group.parentAlias = contentEditingHelper.getParentAlias(group.alias);
            });

            vm.tabs = $filter("filter")($scope.content.tabs, (tab) => {
                return tab.type === 1;
            });

            if (vm.tabs.length > 0) {
                // if we have tabs and some groups that doesn't belong to a tab we need to render those on an "Other" tab.
                contentEditingHelper.registerGenericTab($scope.content.tabs);

                setActiveTab($scope.content.tabs[0]);
            }
        });

        function setActiveTab (tab) {
            vm.activeTabAlias = tab.alias;
            vm.tabs.forEach(tab => tab.active = false);
            tab.active = true;
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.Media.Apps.ContentController", MediaAppContentController);
})();
