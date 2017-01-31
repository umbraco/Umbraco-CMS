(function () {
    "use strict";

    function ScriptsCreateController($scope, $location, navigationService) {

        var vm = this;
        var node = $scope.dialogOptions.currentNode;

        vm.creatingFolder = false;
        vm.folderName = "";
        vm.fileExtension = "";

        vm.createFile = createFile;
        vm.showCreateFolder = showCreateFolder;
        vm.createFolder = createFolder;

        function createFile(fileExtension) {
            $location.search('create', null);
            $location.search('fileExtension', null);
            $location.path("/settings/scripts/edit/" + node.id).search("create", "true").search("fileExtension", fileExtension);
            navigationService.hideMenu();
        }

        function showCreateFolder() {
            vm.creatingFolder = true;
        }

        function createFolder() {
            alert("create folder");
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.Scripts.CreateController", ScriptsCreateController);
})();
