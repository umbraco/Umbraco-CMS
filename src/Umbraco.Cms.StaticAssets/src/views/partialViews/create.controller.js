(function () {
    "use strict";

    function PartialViewsCreateController($scope, codefileResource, $location, navigationService, formHelper, appState) {

        var vm = this;
        var node = $scope.currentNode;

        vm.snippets = [];
        vm.showSnippets = false;
        vm.creatingFolder = false;
        vm.createFolderError = "";
        vm.folderName = "";

        vm.createPartialView = createPartialView;
        vm.showCreateFolder = showCreateFolder;
        vm.createFolder = createFolder;
        vm.showCreateFromSnippet = showCreateFromSnippet;
        vm.close = close;

        function onInit() {
            codefileResource.getSnippets('partialViews')
                .then(function(snippets) {
                    vm.snippets = snippets;
                });
        }

        function createPartialView(selectedSnippet) {

            var snippet = null;

            if(selectedSnippet && selectedSnippet.fileName) {
                snippet = selectedSnippet.fileName;
            }

            $location.path("/settings/partialViews/edit/" + node.id).search("create", "true").search("snippet", snippet);
            navigationService.hideMenu();

        }

        function showCreateFolder() {
            vm.creatingFolder = true;
        }

        function createFolder(form) {
            if (formHelper.submitForm({scope: $scope, formCtrl: form })) {

                codefileResource.createContainer("partialViews", node.id, vm.folderName).then(function(saved) {

                    navigationService.hideMenu();

                    navigationService.syncTree({
                        tree: "partialViews",
                        path: saved.path,
                        forceReload: true,
                        activate: true
                    });

                    formHelper.resetForm({ scope: $scope, formCtrl: form });

                    var section = appState.getSectionState("currentSection");

                }, function(err) {

                    formHelper.resetForm({ scope: $scope, formCtrl: form, hasErrors: true });
                    vm.createFolderError = err;
                });
            }
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

    angular.module("umbraco").controller("Umbraco.Editors.PartialViews.CreateController", PartialViewsCreateController);
})();
