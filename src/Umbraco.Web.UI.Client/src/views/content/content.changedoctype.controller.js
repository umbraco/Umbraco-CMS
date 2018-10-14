(function () {
    "use strict";

    function ChangeDocTypeController($scope, $location, $route, contentResource, contentTypeResource) {

        var vm = this;

        vm.save = save;
        vm.closeDialog = closeDialog;
        vm.repopulate = repopulate;

        vm.currentDocType = null;

        vm.docTypes = [];
        vm.docType = null;

        vm.templateTypes = [];
        vm.templateType = null;

        vm.contentType = null;

        vm.properties = [];
        vm.currentContentType = null;

        vm.compositeContentTypes = [];
        vm.docTypesLength = 0;

        vm.error = null;
        vm.success = false;

        function activate() {
            contentResource.getById($scope.currentNode.id).then(function (data) {
                vm.currentDocType = data;
                populateListOfValidAlternateDocumentTypes().then(function (result) {
                    if (result) {
                        populateListOfTemplates(vm.docType.id).then(function () {
                            populatePropertyMappingWithSources(vm.currentDocType.documentType.id).then(function () {
                                populatePropertyMappingWithDestinations();
                            });
                        });
                    }
                });
            });
        }

        function repopulate(id) {
            populateListOfTemplates(id).then(function () {
                populatePropertyMappingWithDestinations();
            });
        }

        function closeDialog() {
            $scope.nav.hideDialog();
            $location.path("/content/content/edit/" + $scope.currentNode.id);
            $route.reload();
        }

        function save() {
            var data = {
                Id: $scope.currentNode.id,
                NewDocType: vm.docType.id,
                NewTemplateType: vm.templateType.id,
                PropertyMappings: vm.properties.map(function(p) {
                    return {
                        FromName: p.label,
                        FromAlias: p.alias,
                        ToName: p.destination.label,
                        ToAlias: p.destination.alias
                    };
                }),
                Success: false
        };

            contentResource.saveChangeDocType(data).then(function (returnData) {
                vm.success = returnData.Success;
                vm.error = returnData.ErrorMessage;
            });
        }

        function populateListOfValidAlternateDocumentTypes() {
            return contentTypeResource.getAllowedTypes($scope.currentNode.id).then(function (data) {
                vm.docTypes = data;
                vm.docTypesLength = vm.docTypes.length; 
                if (vm.docTypesLength > 0) {
                    vm.docType = vm.docTypes[0];
                    return true;
                }
                return false;
            });
        }

        function populateListOfTemplates(id) {
            return contentTypeResource.getById(id).then(function (data) {
                vm.contentType = data;
                vm.templateTypes = data.allowedTemplates;
                if (vm.templateTypes.length > 0) {
                    vm.templateType = vm.templateTypes[0];
                    vm.templateTypes.unshift({ name: "None" });
                }
                return true;
            });
        }

        function populatePropertyMappingWithSources(id) {
            return contentTypeResource.getById(id).then(function (data) {
                vm.properties = data.groups[0].properties;
            });
        }

        function populatePropertyMappingWithDestinations() {
            angular.forEach(vm.properties, function (prop) {
                prop.destinations = [];
                prop.destination = null;
                angular.forEach(vm.contentType.groups[0].properties, function (innerProp) {
                    if (prop.editor === innerProp.editor) {
                        prop.destinations.push(innerProp);
                    }
                });
                prop.destination = prop.destinations[0];
                prop.destinations.unshift({ label: "None" });
            });

        }

        activate();
    }
    angular.module("umbraco").controller("Umbraco.Editors.Content.ChangeDocTypeController", ChangeDocTypeController);
})();
