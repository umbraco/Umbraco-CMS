(function () {
    "use strict";

    function PropertyNameController($scope) {
        
        var vm = this;
        var element = angular.element($scope.currentStep.element);

        vm.error = false;
        
        vm.initNextStep = initNextStep;

        function initNextStep() {
            if(element.val() === 'Welcome Text') {
                $scope.nextStep();
            } else {
                vm.error = true;
            }
        }

    }

    angular.module("umbraco").controller("Umbraco.Tours.UmbIntroCreateDocType.PropertyNameController", PropertyNameController);
})();
