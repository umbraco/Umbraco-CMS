(function () {
    "use strict";

    function TemplatesTreeController($scope) {

        var eventElement = $($scope.model.currentStep.eventElement);

        function onInit() {
            // check if tree is already open - if it is - go to next step
            if (eventElement.hasClass("icon-navigation-down")) {
                $scope.model.nextStep();
            }
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Tours.UmbIntroRenderInTemplate.TemplatesTreeController", TemplatesTreeController);
})();
