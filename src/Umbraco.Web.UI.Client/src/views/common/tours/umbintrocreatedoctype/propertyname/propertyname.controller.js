(function () {
    "use strict";

    function PropertyNameController($scope) {

        var vm = this;
        var element = $($scope.model.currentStep.element);

        vm.error = false;

        vm.initNextStep = initNextStep;

        function initNextStep() {
            if (element.val().toLowerCase() === 'welcome text') {
                $scope.model.nextStep();
            } else {
                vm.error = true;
            }
        }

    }

    angular.module("umbraco").controller("Umbraco.Tours.UmbIntroCreateDocType.PropertyNameController", PropertyNameController);
})();
