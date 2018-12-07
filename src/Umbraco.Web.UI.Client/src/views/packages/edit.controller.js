(function () {
    "use strict";

    function EditController($location, $routeParams, entityResource, packageResource, contentTypeResource, templateResource, stylesheetResource, languageResource, dictionaryResource, dataTypeResource, editorService) {

        const vm = this;

        vm.showBackButton = true;

        // open all expansion panels
        vm.propertiesOpen = true;
        vm.contentOpen = true;
        vm.filesOpen = true;
        vm.actionsOpen = true;

        vm.back = back;
        vm.createPackage = createPackage;
        vm.save = save;
        vm.removeContentItem = removeContentItem;
        vm.openContentPicker = openContentPicker;
        vm.openFilePicker = openFilePicker;
        vm.removeFile = removeFile;
        vm.openControlPicker = openControlPicker;
        vm.removeControl = removeControl;

        function onInit() {

            const packageId = $routeParams.id;
            const create = $routeParams.create;

            if(create) {
                //pre populate package with some values
                vm.package = {
                    "version": "0.0.1",
                    "license": "MIT License",
                    "licenseUrl": "http://opensource.org/licenses/MIT",
                    "umbracoVersion": Umbraco.Sys.ServerVariables.application.version
                };
            } else {
                // load package
                packageResource.getCreatedById(packageId).then(createdPackage => {
                    vm.package = createdPackage;

                    // get render model for content node
                    if(vm.package.contentNodeId) {
                        entityResource.getById(vm.package.contentNodeId, "Document")
                            .then((entity) => {
                                vm.contentNodeDisplayModel = entity;
                            });
                    }

                }, angular.noop);
            }

            // get all doc types
            contentTypeResource.getAll().then(documentTypes => {
                vm.documentTypes = documentTypes;
            });

            // get all templates
            templateResource.getAll().then(templates => {
                vm.templates = templates;
            });

            // get all stylesheets
            stylesheetResource.getAll().then(stylesheets => {
                vm.stylesheets = stylesheets;
            });

            // TODO: implement macros
            vm.macros = [];

            // get all languages
            languageResource.getAll().then(languages => {
                vm.languages = languages;
            });

            // get all dictionary items
            dictionaryResource.getList().then(dictionaryItems => {
                vm.dictionaryItems = dictionaryItems;
            });

            // get all data types items
            dataTypeResource.getAll().then(dataTypes => {
                vm.dataTypes = dataTypes;
            });

        }

        function back() {
            $location.path("packages/packages/overview").search('create', null);;
        }

        function createPackage() {

            vm.createPackageButtonState = "busy";

            packageResource.createPackage(vm.package).then((updatedPackage) => {
                vm.package = updatedPackage;
                vm.createPackageButtonState = "success";
            }, function(error){
                vm.createPackageButtonState = "error";
            });
        }

        function save() {
            console.log("save package");
        }

        function removeContentItem() {
            vm.package.contentNodeId = null;
        }

        function openContentPicker() {
            const contentPicker = {
                submit: function(model) {
                    if(model.selection && model.selection.length > 0) {
                        vm.package.contentNodeId = model.selection[0].id.toString();
                        vm.contentNodeDisplayModel = model.selection[0];
                    }
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.contentPicker(contentPicker);
        }

        function openFilePicker() {
            const filePicker = {
                title: "Select files",
                section: "settings",
                treeAlias: "files",
                entityType: "file",
                multiPicker: true,
                onlyInitialized: false,
                submit: function(model) {
                    console.log(model.selection);

                    if(model && model.selection) {
                        vm.package.files = vm.package.files ? vm.package.files : [];
                        model.selection.forEach(selected => {
                            vm.package.files.push(selected);
                        });
                    }
                    
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.treePicker(filePicker);
        }

        function removeFile(index) {
            vm.package.files.splice(index, 1);
        }

        function openControlPicker() {
            const controlPicker = {
                title: "Select control",
                treeAlias: "files",
                section:"settings",
                entityType: "file",
                submit: function(model) {
                    if(model.selection && model.selection.length > 0) {
                        vm.package.control = model.selection[0];
                    }
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.contentPicker(controlPicker);
        }

        function removeControl() {
            vm.package.control = null;
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.EditController", EditController);

})();
