(function () {
    "use strict";

    function NodeNameController($scope) {
        
        var vm = this;
        var element = angular.element($scope.currentStep.element);

        vm.error = false;
        
        vm.initNextStep = initNextStep;

        function initNextStep() {
            if(element.val() === 'Home') {
                $scope.nextStep();
            } else {
                vm.error = true;
            }
        }

    }

    angular.module("umbraco").controller("Umbraco.Tours.umbIntroCreateDocType.NodeNameController", NodeNameController);
})();
