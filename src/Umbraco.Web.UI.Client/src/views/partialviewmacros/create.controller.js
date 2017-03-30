(function () {
    "use strict";

    function PartialViewMacrosCreateController($scope, codefileResource, macroResource, $location, navigationService, formHelper, localizationService, appState) {

        var vm = this;
        var node = $scope.dialogOptions.currentNode;
        var localizeCreateFolder = localizationService.localize("defaultdialog_createFolder");

        vm.snippets = [];
        vm.snippet = "Empty";
        vm.createMacro = false;
        vm.createFolderError = "";
        vm.folderName = "";
        vm.fileName = "";

        vm.creatingFolder = false;
        vm.creatingFile = false;

        vm.showCreateFolder = showCreateFolder;
        vm.showCreateFile = showCreateFile;
        vm.createFolder = createFolder;
        vm.createFile = createFile;

        function onInit() {
            codefileResource.getSnippets('partialViewMacros')
                .then(function (snippets) {
                    vm.snippets = snippets;
                });
        }

        function showCreateFolder() {
            vm.creatingFolder = true;
        }

        function showCreateFile() {
            vm.creatingFile = true;
        }

        function createFolder(form) {
            if (formHelper.submitForm({scope: $scope, formCtrl: form, statusMessage: localizeCreateFolder})) {

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

        function createFile(form) {
            if (formHelper.submitForm({ scope: $scope, formCtrl: form, statusMessage: 'create file' })) {

                if (vm.createMacro) {
                    var path = decodeURIComponent(node.id);
                    macroResource.createPartialViewMacroWithFile(path, vm.fileName).then(function(created) {
                        $location.path("/developer/partialviewmacros/edit/" + node.id).search("create", "true").search("name", vm.fileName).search("snippet", vm.snippet);
                        navigationService.hideMenu();
                    }, function(err) {
                        vm.createFileError = err;

                        //show any notifications
                        if (angular.isArray(err.data.notifications)) {
                            for (var i = 0; i < err.data.notifications.length; i++) {
                                notificationsService.showNotification(err.data.notifications[i]);
                            }
                        }
                    });
                } else {
                    $location.path("/developer/partialviewmacros/edit/" + node.id).search("create", "true").search("name", vm.fileName).search("snippet", vm.snippet);
                    navigationService.hideMenu();
                }
            }
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PartialViewMacros.CreateController", PartialViewMacrosCreateController);
})();
