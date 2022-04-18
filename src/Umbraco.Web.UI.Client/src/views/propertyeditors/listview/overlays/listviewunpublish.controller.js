(function () {
    "use strict";

    function ListViewUnpublishController($scope, $routeParams, localizationService) {

        var vm = this;
        vm.loading = true;
        vm.warningText = null;

        vm.changeSelection = changeSelection;
        
        function changeSelection(language) {

            // disable submit button if nothing is selected
            var firstSelected = _.find(vm.languages, function (language) {
                return language.unpublish;
            });

            $scope.model.disableSubmitButton = !firstSelected;

            if (language.isMandatory) {
                $scope.model.languages.forEach(function (lang) {
                    if (lang !== language) {
                        lang.unpublish = true;
                        lang.disabled = language.unpublish;
                    }
                });
            }
        }

        function onInit() {

            vm.selection = $scope.model.selection;
            vm.languages = $scope.model.languages;
            $scope.model.hideSubmitButton = true;

            if (!$scope.model.title) {
                localizationService.localize("content_unpublish").then(function (value) {
                    $scope.model.title = value;
                });
            }

            // node has variants
            if (vm.languages && vm.languages.length > 0) {

                var culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;

                if(culture) {
                    
                    // sort languages so the active on is on top
                    vm.languages = _.sortBy(vm.languages, function (language) {
                        return language.culture === culture ? 0 : 1;
                    });
                    
                    var active = _.find(vm.languages, function (language) {
                        return language.culture === culture;
                    });

                    if (active) {
                        //ensure that the current one is selected
                        active.unpublish = true;
                        changeSelection(active);
                    }

                }
            }

            vm.loading = false;
        }

        vm.checkingReferencesComplete = () => {
            $scope.model.hideSubmitButton = false;
        };

        vm.onReferencesWarning = () => {
            $scope.model.submitButtonStyle = "danger";
          
            // check if the unpublishing of items that have references has been disabled
            if (Umbraco.Sys.ServerVariables.umbracoSettings.disableUnpublishWhenReferenced) {
                // this will only be disabled if we have a warning, indicating that this item or its descendants have reference
                $scope.model.disableSubmitButton = true;
            }

            localizationService.localize("content_unpublish").then(function (action) {
                localizationService.localize("references_listViewDialogWarning", [action.toLowerCase()]).then((value) => {
                    vm.warningText = value;
                });
            });
        };

        onInit();

        //when this dialog is closed, reset all 'publish' flags
        $scope.$on('$destroy', function () {
            if(vm.languages && vm.languages.length > 0) {
                for (var i = 0; i < vm.languages.length; i++) {
                    vm.languages[i].unpublish = false;
                    vm.languages[i].save = false;
                }
            }
        });
    }

    angular.module("umbraco").controller("Umbraco.Overlays.ListViewUnpublishController", ListViewUnpublishController);

})();
