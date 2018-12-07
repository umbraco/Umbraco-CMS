(function () {
    "use strict";

    function EditController($scope, $location, $routeParams, entityResource, packageResource, contentTypeResource, templateResource, stylesheetResource, languageResource, dictionaryResource, dataTypeResource, editorService, formHelper) {

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
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                documentTypes.forEach(documentType => {
                    documentType.id = documentType.id.toString();
                });
                vm.documentTypes = documentTypes;
            });

            // get all templates
            templateResource.getAll().then(templates => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                templates.forEach(template => {
                    template.id = template.id.toString();
                });
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
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                languages.forEach(language => {
                    language.id = language.id.toString();
                });
                vm.languages = languages;
            });

            // get all dictionary items
            dictionaryResource.getList().then(dictionaryItems => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                dictionaryItems.forEach(dictionaryItem => {
                    dictionaryItem.id = dictionaryItem.id.toString();
                });
                vm.dictionaryItems = dictionaryItems;
            });

            // get all data types items
            dataTypeResource.getAll().then(dataTypes => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                dataTypes.forEach(dataType => {
                    dataType.id = dataType.id.toString();
                });
                vm.dataTypes = dataTypes;
            });

        }

        function back() {
            $location.path("packages/packages/overview").search('create', null);;
        }

        function createPackage(editPackageForm) {

            if (formHelper.submitForm({ formCtrl: editPackageForm, scope: $scope })) {

                vm.createPackageButtonState = "busy";

                packageResource.createPackage(vm.package).then((updatedPackage) => {
                    vm.package = updatedPackage;
                    vm.createPackageButtonState = "success";
                }, function(err){
                    formHelper.handleError(err);
                    vm.createPackageButtonState = "error";
                });
            }
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

            let selection = angular.copy(vm.package.files);

            const filePicker = {
                title: "Select files",
                section: "settings",
                treeAlias: "files",
                entityType: "file",
                multiPicker: true,
                onlyInitialized: false,
                select: function(node) {
                    node.selected = !node.selected;

                    const id = unescape(node.id);
                    const index = selection.indexOf(id);

                    if(node.selected) {
                        if(index === -1) {
                            selection.push(id);
                        }
                    } else {
                        selection.splice(index, 1);
                    }
                },
                submit: function() {
                    vm.package.files = selection;
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
                section: "settings",
                treeAlias: "files",
                entityType: "file",
                onlyInitialized: false,
                select: function(node) {
                    const id = unescape(node.id);
                    vm.package.loadControl = id;
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.treePicker(controlPicker);
        }

        function removeControl() {
            vm.package.loadControl = null;
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.EditController", EditController);

})();
