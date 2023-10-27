(function () {
    "use strict";

    function NodePermissionsController($scope, localizationService) {

        var vm = this;

        vm.submit = submit;
        vm.close = close;

        function onInit() {
            // set default title
            if(!$scope.model.title) {
                localizationService.localize("defaultdialogs_permissionsEdit").then(function(value){
                    $scope.model.title = value + " " + $scope.model.node.name;
                });
            }
        }

        function submit() {
            if($scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }

        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.NodePermissionsController", NodePermissionsController);

})();
