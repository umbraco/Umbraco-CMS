(function () {
    "use strict";

    function StyleSheetsCreateController($scope, $location, navigationService) {

        var vm = this;
        var node = $scope.dialogOptions.currentNode;

        vm.createFile = createFile;

        function createFile() {
            $location.path("/settings/stylesheets/edit/" + node.id).search("create", "true");
            navigationService.hideMenu();
        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.StyleSheets.CreateController", StyleSheetsCreateController);
})();
