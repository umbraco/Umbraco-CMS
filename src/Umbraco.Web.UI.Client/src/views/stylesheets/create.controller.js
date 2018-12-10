(function () {
    "use strict";

    function StyleSheetsCreateController($scope, $location, navigationService) {

        var vm = this;
        var node = $scope.currentNode;

        vm.createFile = createFile;
        vm.createRichtextStyle = createRichtextStyle;
        vm.close = close;

        function createFile() {
            $location.path("/settings/stylesheets/edit/" + node.id).search("create", "true");
            navigationService.hideMenu();
        }

        function createRichtextStyle() {
            $location.path("/settings/stylesheets/edit/" + node.id).search("create", "true").search("rtestyle", "true");
            navigationService.hideMenu();
        }

        function close() {
            const showMenu = true;
            navigationService.hideDialog(showMenu);
        }
        
    }

    angular.module("umbraco").controller("Umbraco.Editors.StyleSheets.CreateController", StyleSheetsCreateController);
})();
