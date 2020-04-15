//used for the media picker dialog
angular.module("umbraco")
.controller("Umbraco.Editors.BlockEditorController",
    function ($scope, localizationService) {
        var vm = this;

        vm.content = $scope.model.content;
        vm.settings = $scope.model.settings;

        vm.model = $scope.model;

        vm.tabs = [];

        if (vm.content && vm.content.variants) {

            var apps = vm.content.apps;

            vm.tabs = apps;

            // replace view of content app.
            var contentApp = apps.find(entry => entry.alias === "umbContent");
            contentApp.view = "views/common/infiniteeditors/elementeditor/elementeditor.content.html";
            
            if($scope.model.hideContent) {
                apps.splice(apps.indexOf(contentApp), 1);
            }

            // remove info app:
            var infoAppIndex = apps.findIndex(entry => entry.alias === "umbInfo");
            apps.splice(infoAppIndex, 1);
            
        }

        if (vm.settings && vm.settings.variants) {
            localizationService.localize("blockEditor_tabBlockSettings").then(
                function (settingsName) {
                    var settingsTab = {
                        "name": settingsName,
                        "alias": "settings",
                        "icon": "icon-settings",
                        "view": "views/common/infiniteeditors/elementeditor/elementeditor.settings.html"
                    };
                    vm.tabs.push(settingsTab);
                }
            );
        }

        // activate first app:
        if (vm.tabs.length > 0) {
            vm.tabs[0].active = true;
        }

        vm.submitAndClose = function () {
            if ($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }

        vm.close = function() {
            if ($scope.model && $scope.model.close) {
                // TODO: If content has changed, we should notify user.
                $scope.model.close($scope.model);
            }
        }

    }
);
