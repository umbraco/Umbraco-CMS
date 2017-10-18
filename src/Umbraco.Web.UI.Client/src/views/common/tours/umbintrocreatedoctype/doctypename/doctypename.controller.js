(function () {
    "use strict";

    function DocTypeNameController($scope) {
        
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

    angular.module("umbraco").controller("Umbraco.Tours.UmbIntroCreateDocType.DocTypeNameController", DocTypeNameController);
})();
