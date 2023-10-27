(function () {
    "use strict";

    function ScriptsCreateController($scope, $location, navigationService, formHelper, codefileResource, localizationService, appState) {

        var vm = this;
        var node = $scope.currentNode;

        vm.creatingFolder = false;
        vm.folderName = "";
        vm.createFolderError = "";
        vm.fileExtension = "";

        vm.createFile = createFile;
        vm.showCreateFolder = showCreateFolder;
        vm.createFolder = createFolder;
        vm.close = close;

        function createFile() {
            $location.path("/settings/scripts/edit/" + node.id).search("create", "true");
            navigationService.hideMenu();
        }

        function showCreateFolder() {
            vm.creatingFolder = true;
        }

        function createFolder(form) {

            if (formHelper.submitForm({ scope: $scope, formCtrl: form })) {

                codefileResource.createContainer("scripts", node.id, vm.folderName).then(function (saved) {

                    navigationService.hideMenu();

                    navigationService.syncTree({
                        tree: "scripts",
                        path: saved.path,
                        forceReload: true,
                        activate: true
                    });

                    formHelper.resetForm({ scope: $scope, formCtrl: form });

                    var section = appState.getSectionState("currentSection");

                }, function (err) {

                    formHelper.resetForm({ scope: $scope, formCtrl: form, hasErrors: true });
                    vm.createFolderError = err;

                });
            }

        }

        function close() {
            const showMenu = true;
            navigationService.hideDialog(showMenu);
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.Scripts.CreateController", ScriptsCreateController);
})();
