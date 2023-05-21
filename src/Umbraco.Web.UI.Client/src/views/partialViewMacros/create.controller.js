(function () {
    "use strict";

    function PartialViewMacrosCreateController($scope, codefileResource, $location, navigationService, formHelper, appState) {

        var vm = this;
        var node = $scope.currentNode;

        vm.snippets = [];
        vm.createFolderError = "";
        vm.folderName = "";
        vm.fileName = "";
        vm.showSnippets = false;
        vm.creatingFolder = false;

        vm.showCreateFolder = showCreateFolder;
        vm.createFolder = createFolder;
        vm.createFile = createFile;
        vm.createFileWithoutMacro = createFileWithoutMacro;
        vm.showCreateFromSnippet = showCreateFromSnippet;
        vm.createFileFromSnippet = createFileFromSnippet;
        vm.close = close;

        function onInit() {
            codefileResource.getSnippets('partialViewMacros')
                .then(function (snippets) {
                    vm.snippets = snippets;
                });
        }

        function showCreateFolder() {
            vm.creatingFolder = true;
        }

        function createFolder(form) {
            if (formHelper.submitForm({ scope: $scope, formCtrl: form })) {

                codefileResource.createContainer("partialViewMacros", node.id, vm.folderName).then(function (saved) {

                    navigationService.hideMenu();

                    navigationService.syncTree({
                        tree: "partialViewMacros",
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

        function createFile() {
            $location.path("/settings/partialViewMacros/edit/" + node.id).search("create", "true");
            navigationService.hideMenu();
        }

        function createFileWithoutMacro() {
            $location.path("/settings/partialViewMacros/edit/" + node.id).search("create", "true").search("nomacro", "true");
            navigationService.hideMenu();
        }

        function createFileFromSnippet(snippet) {
            $location.path("/settings/partialViewMacros/edit/" + node.id).search("create", "true").search("snippet", snippet.fileName);
            navigationService.hideMenu();
        }

        function showCreateFromSnippet() {
            vm.showSnippets = true;
        }

        function close() {
            const showMenu = true;
            navigationService.hideDialog(showMenu);
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PartialViewMacros.CreateController", PartialViewMacrosCreateController);
})();
