(function () {
    "use strict";

    function NodeNameController($scope) {

        var vm = this;
        var element = $($scope.model.currentStep.element);

        vm.error = false;

        vm.initNextStep = initNextStep;

        function initNextStep() {
            if (element.val().toLowerCase() === 'home') {
                $scope.model.nextStep();
            } else {
                vm.error = true;
            }
        }

    }

    angular.module("umbraco").controller("Umbraco.Tours.UmbIntroCreateContent.NodeNameController", NodeNameController);
})();
