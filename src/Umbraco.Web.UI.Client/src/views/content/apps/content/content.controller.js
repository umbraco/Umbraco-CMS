(function () {
    "use strict";

    function ContentAppContentController($scope, $timeout, serverValidationManager) {

        var vm = this;
        vm.loading = true;

        function onInit() {
            vm.content = $scope.model.viewModel;
            serverValidationManager.notify();
            vm.loading = false;
        }

        onInit();

        //if this variant has a culture/language assigned, then we need to watch it since it will change
        //if the language drop down changes and we need to re-init
        if ($scope.model.viewModel.language) {
            $scope.$watch(function () {
                return $scope.model.viewModel.language.culture;
            }, function (newVal, oldVal) {
                if (newVal !== oldVal) {
                    vm.loading = true;

                    //TODO: Can we minimize the flicker?
                    $timeout(function () {
                        onInit();
                    }, 100);
                }
            });
        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.Content.Apps.ContentController", ContentAppContentController);
})();
