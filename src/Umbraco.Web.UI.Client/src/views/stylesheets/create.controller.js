(function () {
    "use strict";

    function StyleSheetsCreateController($scope, $location, navigationService) {

        var vm = this;
        var node = $scope.currentNode;

        vm.createFile = createFile;
        vm.createRichtextStyle = createRichtextStyle;

        function createFile() {
            $location.path("/settings/stylesheets/edit/" + node.id).search("create", "true");
            navigationService.hideMenu();
        }

        function createRichtextStyle() {
            $location.path("/settings/stylesheets/edit/" + node.id).search("create", "true").search("rtestyle", "true");
            navigationService.hideMenu();
        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.StyleSheets.CreateController", StyleSheetsCreateController);
})();
