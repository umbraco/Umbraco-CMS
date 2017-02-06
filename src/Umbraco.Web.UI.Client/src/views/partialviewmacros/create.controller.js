(function () {
    "use strict";

    function PartialViewMacrosCreateController($scope, codefileResource, $location, navigationService, formHelper, localizationService) {

        var vm = this;
        var node = $scope.dialogOptions.currentNode;
        var localizeCreateFolder = localizationService.localize("defaultdialog_createFolder");

        vm.snippets = [];
        vm.showSnippets = false;
        vm.creatingFolder = false;
        vm.createFolderError = "";
        vm.folderName = "";

        vm.createPartialViewMacro = createPartialViewMacro;
        vm.showCreateFolder = showCreateFolder;
        vm.createFolder = createFolder;
        vm.showCreateFromSnippet = showCreateFromSnippet;

        function onInit() {
            codefileResource.getSnippets('partialViewMacros')
                .then(function(snippets) {
                    vm.snippets = snippets;
                });
        }

        function createPartialViewMacro(selectedSnippet) {

            var snippet = null;

            if(selectedSnippet && selectedSnippet.fileName) {
                snippet = selectedSnippet.fileName;
            }

            $location.path("/developer/partialviewmacros/edit/" + node.id).search("create", "true").search("snippet", snippet);
            navigationService.hideMenu();

        }

        function showCreateFolder() {
            vm.creatingFolder = true;
        }

        function createFolder(form) {
            if (formHelper.submitForm({scope: $scope, formCtrl: form, statusMessage: localizeCreateFolder})) {

                codefileResource.createContainer("partialViewMacros", node.id, vm.folderName).then(function(path) {

                    navigationService.hideMenu();

                    navigationService.syncTree({
                        tree: "partialViewMacros",
                        path: path,
                        forceReload: true,
                        activate: true
                    });

                    formHelper.resetForm({
                        scope: $scope
                    });

                    var section = appState.getSectionState("currentSection");

                }, function(err) {

                    vm.createFolderError = err;

                    //show any notifications
                    if (angular.isArray(err.data.notifications)) {
                        for (var i = 0; i < err.data.notifications.length; i++) {
                            notificationsService.showNotification(err.data.notifications[i]);
                        }
                    }
                });
            }
        }
        
        function showCreateFromSnippet() {
            vm.showSnippets = true;
        }
        
        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PartialViewMacros.CreateController", PartialViewMacrosCreateController);
})();
