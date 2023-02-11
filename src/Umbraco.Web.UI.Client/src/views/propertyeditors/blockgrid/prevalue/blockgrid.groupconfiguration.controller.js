(function () {
    "use strict";

    function GroupConfigurationController($scope) {

        var vm = this;
        vm.addGroup = function() {
            $scope.model.value.push({
                key: String.CreateGuid(),
                name: "Unnamed group"
            });
        }

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockGrid.GroupConfigurationController", GroupConfigurationController);

})();
