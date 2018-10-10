(function () {
    "use strict";

    function PublishDescendantsController($scope, localizationService) {

        var vm = this;

        function onInit() {

            vm.variants = $scope.model.variants;

            if (!$scope.model.title) {
                localizationService.localize("buttons_publishDescendants").then(function (value) {
                    $scope.model.title = value;
                });
            }
            
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.PublishDescendantsController", PublishDescendantsController);

})();
