(function () {
    "use strict";

    function PartialViewMacrosCreateController($scope, codefileResource, macroResource, $location, navigationService, formHelper, localizationService, appState) {

        var vm = this;
        var node = $scope.dialogOptions.currentNode;
        var localizeCreateFolder = localizationService.localize("defaultdialog_createFolder");

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
            if (formHelper.submitForm({ scope: $scope, formCtrl: form, statusMessage: localizeCreateFolder })) {

                codefileResource.createContainer("partialViewMacros", node.id, vm.folderName).then(function (saved) {

                    navigationService.hideMenu();

                    navigationService.syncTree({
                        tree: "partialViewMacros",
                        path: saved.path,
                        forceReload: true,
                        activate: true
                    });

                    formHelper.resetForm({
                        scope: $scope
                    });

                    var section = appState.getSectionState("currentSection");

                }, function (err) {

                    vm.createFolderError = err;

                    //show any notifications
                    formHelper.showNotifications(err.data);    
                });
            }
        }

        function createFile() {
            $location.path("/developer/partialviewmacros/edit/" + node.id).search("create", "true");
            navigationService.hideMenu();
        }

        function createFileWithoutMacro() {
            $location.path("/developer/partialviewmacros/edit/" + node.id).search("create", "true").search("nomacro", "true");
            navigationService.hideMenu();
        }

        function createFileFromSnippet(snippet) {
            $location.path("/developer/partialviewmacros/edit/" + node.id).search("create", "true").search("snippet", snippet.fileName);
            navigationService.hideMenu();
        }

        function showCreateFromSnippet() {
            vm.showSnippets = true;
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PartialViewMacros.CreateController", PartialViewMacrosCreateController);
})();
