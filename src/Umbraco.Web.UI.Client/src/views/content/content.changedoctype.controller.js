(function () {
    "use strict";

    function ChangeDocTypeController($scope, contentResource, contentTypeResource) {

        var vm = this;

        vm.closeDialog = closeDialog;

        vm.currentName = null;
        vm.currentDocType = null;

        vm.docTypes = [];
        vm.docType = null;

        function activate() { 
            populateListOfValidAlternateDocumentTypes();
        }

        function closeDialog() {
            $scope.nav.hideDialog();
        }

        function populateListOfValidAlternateDocumentTypes() {
            contentTypeResource.getAll()
                .then(function (data) {
                    vm.docTypes = data;
                    if (vm.docTypes.length > 0) {
                        vm.docType = vm.docTypes[0]; 
                    }
                });
        }
        activate();
    }
    angular.module("umbraco").controller("Umbraco.Editors.Content.ChangeDocTypeController", ChangeDocTypeController);
})();
