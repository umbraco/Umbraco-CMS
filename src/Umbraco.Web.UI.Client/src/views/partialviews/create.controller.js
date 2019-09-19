(function () {
    "use strict";

    function PartialViewsCreateController($scope, codefileResource, $location, navigationService, formHelper, localizationService, appState) {

        var vm = this;
        var node = $scope.dialogOptions.currentNode;
        var localizeCreateFolder = localizationService.localize("defaultdialog_createFolder");

        vm.snippets = [];
        vm.showSnippets = false;
        vm.creatingFolder = false;
        vm.createFolderError = "";
        vm.folderName = "";

        vm.createPartialView = createPartialView;
        vm.showCreateFolder = showCreateFolder;
        vm.createFolder = createFolder;
        vm.showCreateFromSnippet = showCreateFromSnippet;

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

            $location.path("/settings/partialviews/edit/" + node.id).search("create", "true").search("snippet", snippet);
            navigationService.hideMenu();

        }

        function showCreateFolder() {
            vm.creatingFolder = true;
        }

        function createFolder(form) {
            if (formHelper.submitForm({scope: $scope, formCtrl: form, statusMessage: localizeCreateFolder})) {

                codefileResource.createContainer("partialViews", node.id, vm.folderName).then(function(saved) {

                    navigationService.hideMenu();

                    navigationService.syncTree({
                        tree: "partialViews",
                        path: saved.path,
                        forceReload: true,
                        activate: true
                    });

                    formHelper.resetForm({
                        scope: $scope
                    });

                    var section = appState.getSectionState("currentSection");

                }, function(err) {

                    vm.createFolderError = err;

                    formHelper.showNotifications(err.data);                    
                });
            }
        }
        
        function showCreateFromSnippet() {
            vm.showSnippets = true;
        }
        
        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PartialViews.CreateController", PartialViewsCreateController);
})();
