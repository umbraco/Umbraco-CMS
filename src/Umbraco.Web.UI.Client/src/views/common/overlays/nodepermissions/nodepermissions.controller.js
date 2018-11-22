(function () {
    "use strict";

    function NodePermissionsController($scope, localizationService) {

        var vm = this;

        function onInit() {

            // set default title
            if(!$scope.model.title) {
                localizationService.localize("defaultdialogs_permissionsEdit").then(function(value){
                    $scope.model.title = value + " " + $scope.model.node.name;
                });
            }
            
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.NodePermissionsController", NodePermissionsController);

})();
