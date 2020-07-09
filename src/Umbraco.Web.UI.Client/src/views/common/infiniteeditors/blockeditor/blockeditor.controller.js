angular.module("umbraco")
.controller("Umbraco.Editors.BlockEditorController",
    function ($scope, localizationService, formHelper) {
        var vm = this;

        vm.model = $scope.model;
        vm.model = $scope.model;
        vm.tabs = [];
        localizationService.localizeMany([
            vm.model.liveEditing ? "prompt_discardChanges" : "general_close",
            vm.model.liveEditing ? "buttons_confirmActionConfirm" : "buttons_submitChanges"
        ]).then(function (data) {
            vm.closeLabel = data[0];
            vm.submitLabel = data[1];
        });

        if ($scope.model.content && $scope.model.content.variants) {

            var apps = $scope.model.content.apps;

            vm.tabs = apps;

            // replace view of content app.
            var contentApp = apps.find(entry => entry.alias === "umbContent");
            if (contentApp) {
                contentApp.view = "views/common/infiniteeditors/blockeditor/blockeditor.content.html";
                if(vm.model.hideContent) {
                    apps.splice(apps.indexOf(contentApp), 1);
                } else if (vm.model.openSettings !== true) {
                    contentApp.active = true;
                }
            }

            // remove info app:
            var infoAppIndex = apps.findIndex(entry => entry.alias === "umbInfo");
            apps.splice(infoAppIndex, 1);

        }

        if (vm.model.settings && vm.model.settings.variants) {
            localizationService.localize("blockEditor_tabBlockSettings").then(
                function (settingsName) {
                    var settingsTab = {
                        "name": settingsName,
                        "alias": "settings",
                        "icon": "icon-settings",
                        "view": "views/common/infiniteeditors/blockeditor/blockeditor.settings.html"
                    };
                    vm.tabs.push(settingsTab);
                    if (vm.model.openSettings) {
                        settingsTab.active = true;
                    }
                }
            );
        }

        vm.submitAndClose = function () {
            if (vm.model && vm.model.submit) {
                // always keep server validations since this will be a nested editor and server validations are global
                if (formHelper.submitForm({ scope: $scope, formCtrl: vm.blockForm, keepServerValidation: true })) {
                    vm.model.submit(vm.model);
                }
            }
        }

        vm.close = function() {
            if (vm.model && vm.model.close) {
                // TODO: check if content/settings has changed and ask user if they are sure.
                vm.model.close(vm.model);
            }
        }

    }
);
