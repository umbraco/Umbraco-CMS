(function () {
    "use strict";

    function ChangeDocTypeController($scope, contentResource, contentTypeResource, navigationService, eventsService) {

        var vm = this;

        vm.save = save;
        vm.closeDialog = closeDialog;
        vm.repopulate = repopulate;

        vm.currentDocType = null;

        vm.docTypes = [];
        vm.docType = null;

        vm.templateTypes = [];
        vm.templateType = null;

        vm.destination = null;
        vm.currentContentType = null;

        vm.compositeContentTypes = [];
        vm.docTypesLength = 0;

        function activate() {
            contentResource.getById($scope.currentNode.id).then(function (data) {
                vm.currentDocType = data;
                populateListOfValidAlternateDocumentTypes().then(function (result) {
                    if (result) {
                        populateListOfTemplates(vm.docType.id);
                        populatePropertyMappingWithSources(vm.currentDocType.documentType.id);
                        populatePropertyMappingWithDestinations(vm.docType.id);
                    }
                });
            });
        }

        function repopulate(id) {
            populateListOfTemplates(id);
            populatePropertyMappingWithDestinations(id);
        }

        function closeDialog() {
            $scope.nav.hideDialog();
        }

        function save() {
            var data = {
                Id: $scope.currentNode.id,
                NewDocType: vm.docType.id,
                NewTemplateType: vm.templateType.id,
                PropertyMappings: vm.properties.map(function (p) {
                    return {
                        FromName: p.label,
                        FromAlias: p.alias,
                        ToName: p.destination.label,
                        ToAlias: p.destination.alias,
                        Value: null
                    };
                })
            };

            contentResource.saveChangeDocType(data).then(function () {
                vm.success = data.Success;
                syncTreeNode(vm.currentDocType.path);
                eventsService.emit("content.saved", { content: $scope.content });
                closeDialog();
            }, function (e) {
                console.log(e);
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
            contentTypeResource.getById(id).then(function (data) {
                vm.templateTypes = data.allowedTemplates;
                if (vm.templateTypes.length > 0) {
                    vm.templateType = vm.templateTypes[0];
                    vm.templateTypes.unshift({ name: "None" });
                }
            });
        }

        function populatePropertyMappingWithSources(id) {
            contentTypeResource.getById(id).then(function (data) {
                vm.properties = data.groups[0].properties;
            });
        }

        function populatePropertyMappingWithDestinations(id) {
            contentTypeResource.getById(id).then(function (data) {
                var docTypProps = data.groups[0].properties;
                angular.forEach(vm.properties, function (prop) {
                    prop.destinations = [];
                    prop.destination = null;
                    angular.forEach(docTypProps, function (innerProp) {
                        if (prop.editor === innerProp.editor) {
                            prop.destinations.push(innerProp);
                        }
                    });

                    prop.destination = prop.destinations[0];
                    prop.destinations.unshift({ label: "None" });
                });
            });
        }

        /** Syncs the content type  to it's tree node - this occurs on first load and after saving */
        function syncTreeNode(path) {
            console.log(navigationService);
            navigationService.syncTree({ tree: "content", path: path.split(","), forceReload: true }).then(function (syncArgs) {
                vm.currentNode = syncArgs.node;
            });
            navigationService.reloadNode(vm.currentNode);
        }
        activate();
    }
    angular.module("umbraco").controller("Umbraco.Editors.Content.ChangeDocTypeController", ChangeDocTypeController);
})();
