(function () {
    "use strict";

    function ListViewUnpublishController($scope, $routeParams, localizationService) {

        var vm = this;
        vm.loading = true;

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

            vm.languages = $scope.model.languages;

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
