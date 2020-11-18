(function () {
    "use strict";

    function StyleSheetsCreateController($scope, $location, navigationService, formHelper, codefileResource) {

        var vm = this;
        var node = $scope.currentNode;

        vm.createFile = createFile;
        vm.createRichtextStyle = createRichtextStyle;
        vm.close = close;
        vm.creatingFolder = false;
        vm.showCreateFolder = showCreateFolder;
        vm.createFolder = createFolder;

        function createFile() {
            $location.path("/settings/stylesheets/edit/" + node.id).search("create", "true");
            navigationService.hideMenu();
        }

        function createRichtextStyle() {
            $location.path("/settings/stylesheets/edit/" + node.id).search("create", "true").search("rtestyle", "true");
            navigationService.hideMenu();
        }

        function showCreateFolder() {
            vm.creatingFolder = true;
        }

        function createFolder(form) {

            if (formHelper.submitForm({ scope: $scope, formCtrl: form })) {

                codefileResource.createContainer("stylesheets", node.id, vm.folderName).then(function (saved) {

                    navigationService.hideMenu();

                    navigationService.syncTree({
                        tree: "stylesheets",
                        path: saved.path,
                        forceReload: true,
                        activate: true
                    });

                    formHelper.resetForm({ scope: $scope, formCtrl: form });

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

    angular.module("umbraco").controller("Umbraco.Editors.StyleSheets.CreateController", StyleSheetsCreateController);
})();
