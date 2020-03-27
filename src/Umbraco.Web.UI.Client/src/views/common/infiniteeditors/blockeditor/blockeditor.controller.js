//used for the media picker dialog
angular.module("umbraco")
.controller("Umbraco.Editors.BlockEditorController",
    function ($scope) {
        var vm = this;

        function showContent() {
            if (vm.settingsTab) vm.settingsTab.active = false;
            vm.contentTab.active = true;
        }

        function showSettings() {
            if (vm.settingsTab) vm.settingsTab.active = true;
            vm.contentTab.active = false;
        }

        vm.content = $scope.model.content;
        vm.settings = $scope.model.settings;
        vm.tabs = [];
        var settingsOnly = vm.content && vm.content.variants ? false : true;

        if (!settingsOnly) {
            vm.contentTab = {
                "name": "Content",
                "alias": "content",
                "icon": "icon-document",
                "action": showContent,
                "active": true
            };

            vm.tabs.push(vm.contentTab);
        }

        if (vm.settings && vm.settings.variants) {
            vm.settingsTab = {
                "name": "Settings",
                "alias": "settings",
                "icon": "icon-settings",
                "action": showSettings,
                "active": settingsOnly
            };
            vm.tabs.push(vm.settingsTab);
        }

        vm.title = (settingsOnly ? 'SETTINGS: ' : '') + $scope.model.title;

        vm.saveAndClose = function () {
            if ($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }

        vm.close = function() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close($scope.model);
            }
        }

    }
);
