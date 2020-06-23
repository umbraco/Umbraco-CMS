//used for the media picker dialog
angular.module("umbraco")
.controller("Umbraco.Editors.BlockEditorController",
    function ($scope, localizationService, formHelper) {
        var vm = this;

        vm.content = $scope.model.content;
        vm.settings = $scope.model.settings;

        localizationService.localizeMany([
            $scope.model.liveEditing ? "prompt_discardChanges" : "general_close",
            $scope.model.liveEditing ? "buttons_confirmActionConfirm" : "buttons_submitChanges"
        ]).then(function (data) {
            vm.closeLabel = data[0];
            vm.submitLabel = data[1];
        });

        vm.model = $scope.model;

        vm.tabs = [];

        if (vm.content && vm.content.variants) {

            var apps = vm.content.apps;

            vm.tabs = apps;

            // replace view of content app.
            var contentApp = apps.find(entry => entry.alias === "umbContent");
            if(contentApp) {
                contentApp.view = "views/common/infiniteeditors/elementeditor/elementeditor.content.html";
                if($scope.model.hideContent) {
                    apps.splice(apps.indexOf(contentApp), 1);
                } else if ($scope.model.openSettings !== true) {
                    contentApp.active = true;
                }
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
                    if ($scope.model.openSettings) {
                        settingsTab.active = true;
                    }
                }
            );
        }

        vm.submitAndClose = function () {
            if ($scope.model && $scope.model.submit) {
                if (formHelper.submitForm({ scope: $scope })) {
                    $scope.model.submit($scope.model);
                }
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
