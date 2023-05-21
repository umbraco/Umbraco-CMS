angular.module("umbraco")
    .controller("Umbraco.Editors.BlockEditorController",
        function ($scope, localizationService, formHelper, overlayService) {

            var vm = this;

            vm.model = $scope.model;
            vm.tabs = [];

            localizationService.localizeMany([
                vm.model.createFlow ? "general_cancel" : (vm.model.liveEditing ? "prompt_discardChanges" : "general_close"),
                vm.model.createFlow ? "general_create" : (vm.model.liveEditing ? "buttons_confirmActionConfirm" : "buttons_submitChanges")
            ]).then(function (data) {
                vm.closeLabel = data[0];
                vm.submitLabel = data[1];
            });

            if (vm.model.content && vm.model.content.variants) {

                var apps = vm.model.content.apps;

                // configure the content app based on settings
                var contentApp = apps.find(entry => entry.alias === "umbContent");
                if (contentApp) {
                    if (vm.model.hideContent) {
                        apps.splice(apps.indexOf(contentApp), 1);
                    }
                    contentApp.active = (vm.model.openSettings !== true);
                }

                if (vm.model.settings && vm.model.settings.variants) {
                    var settingsApp = apps.find(entry => entry.alias === "settings");
                    if (settingsApp) {
                        settingsApp.active = (vm.model.openSettings === true);
                    }
                }

                var activeApp = apps.filter(x => x.active);
                if (activeApp.length === 0 && apps.length > 0) {
                  apps[0].active = true;
                }

                vm.tabs = apps;
            }

            vm.submitAndClose = function () {
                if (vm.model && vm.model.submit) {

                    // always keep server validations since this will be a nested editor and server validations are global
                    if (formHelper.submitForm({
                        scope: $scope,
                        formCtrl: vm.blockForm,
                        keepServerValidation: true
                    })) {
                        vm.model.submit(vm.model);
                        vm.saveButtonState = "success";
                    } else {
                        vm.saveButtonState = "error";
                    }
                }
            };

            vm.close = function () {
                if (vm.model && vm.model.close) {
                    // TODO: At this stage there could very well have been server errors that have been cleared
                    // but if we 'close' we are basically cancelling the value changes which means we'd want to cancel
                    // all of the server errors just cleared. It would be possible to do that but also quite annoying.
                    // The rudimentary way would be to:
                    // * Track all cleared server errors here by subscribing to the prefix validation of controls contained here
                    // * If this is closed, re-add all of those server validation errors
                    // A more robust way to do this would be to:
                    // * Add functionality to the serverValidationManager whereby we can remove validation errors and it will
                    //      maintain a copy of the original errors
                    // * It would have a 'commit' method to commit the removed errors - which we would call in the formHelper.submitForm when it's successful
                    // * It would have a 'rollback' method to reset the removed errors - which we would call here

                    if (vm.model.createFlow === true || vm.blockForm.$dirty === true) {
                        var labels = vm.model.createFlow === true ? ["blockEditor_confirmCancelBlockCreationHeadline", "blockEditor_confirmCancelBlockCreationMessage"] : ["prompt_discardChanges", "blockEditor_blockHasChanges"];
                        localizationService.localizeMany(labels).then(function (localizations) {
                            const confirm = {
                                title: localizations[0],
                                view: "default",
                                content: localizations[1],
                                submitButtonLabelKey: "general_discard",
                                submitButtonStyle: "danger",
                                closeButtonLabelKey: "prompt_stay",
                                submit: function () {
                                    overlayService.close();
                                    vm.model.close(vm.model);
                                },
                                close: function () {
                                    overlayService.close();
                                }
                            };
                            overlayService.open(confirm);
                        });
                    } else {
                        vm.model.close(vm.model);
                    }

                }
            };

        }
    );
