(function () {
    "use strict";

    function NodePermissionsController($scope, localizationService) {

        var vm = this;

        function onInit() {

            // set default title
            if(!$scope.model.title) {
                $scope.model.title = localizationService.localize("actions_permissionsEdit") + $scope.model.node.name;
            }
            
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.NodePermissionsController", NodePermissionsController);

})();
