//used for the media picker dialog
angular.module("umbraco")
.controller("Umbraco.Editors.BlockEditorController",
    function ($scope, localizationService, formHelper) {
        var vm = this;

        // TODO: Why are we assigning content/setting separately when we already have vm.model?
        vm.content = $scope.model.content;
        vm.settings = $scope.model.settings;        
        vm.model = $scope.model;
        vm.tabs = [];
        localizationService.localizeMany([
            vm.model.liveEditing ? "prompt_discardChanges" : "general_close",
            vm.model.liveEditing ? "buttons_confirmActionConfirm" : "buttons_submitChanges"
        ]).then(function (data) {
            vm.closeLabel = data[0];
            vm.submitLabel = data[1];
        });

        if (vm.content && vm.content.variants) {

            var apps = vm.content.apps;

            vm.tabs = apps;

            // replace view of content app.
            var contentApp = apps.find(entry => entry.alias === "umbContent");
            if (contentApp) {
                // TODO: This is strange, why does this render a view from somewhere else and this is the only place where that view is used?
                contentApp.view = "views/common/infiniteeditors/elementeditor/elementeditor.content.html";
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

        if (vm.settings && vm.settings.variants) {
            localizationService.localize("blockEditor_tabBlockSettings").then(
                function (settingsName) {
                    var settingsTab = {
                        "name": settingsName,
                        "alias": "settings",
                        "icon": "icon-settings",
                        // TODO: This is strange, why does this render a view from somewhere else and this is the only place where that view is used?
                        "view": "views/common/infiniteeditors/elementeditor/elementeditor.settings.html"
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
                if (formHelper.submitForm({ scope: $scope })) {
                    vm.model.submit(vm.model);
                }
            }
        }

        vm.close = function() {
            if (vm.model && vm.model.close) {
                // TODO: If content has changed, we should notify user.
                vm.model.close(vm.model);
            }
        }

    }
);
