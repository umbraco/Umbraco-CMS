(function () {
    "use strict";

    function FolderNameController($scope) {
        
        var vm = this;
        var element = angular.element($scope.model.currentStep.element);

        vm.error = false;
        
        vm.initNextStep = initNextStep;

        function initNextStep() {
            if(element.val() === "My folder") {
                $scope.model.nextStep();
            } else {
                vm.error = true;
            }
        }

    }

    angular.module("umbraco").controller("Umbraco.Tours.UmbIntroMediaSection.FolderNameController", FolderNameController);
})();
