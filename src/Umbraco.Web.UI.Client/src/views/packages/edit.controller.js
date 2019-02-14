(function () {
    "use strict";

    function EditController($scope, $location, $routeParams, umbRequestHelper, entityResource, packageResource, editorService, formHelper, localizationService) {

        const vm = this;

        vm.showBackButton = true;

        // open all expansion panels
        vm.propertiesOpen = true;
        vm.contentOpen = true;
        vm.filesOpen = true;
        vm.actionsOpen = true;
        vm.loading = true;
        vm.back = back;
        vm.createOrUpdatePackage = createOrUpdatePackage;
        vm.removeContentItem = removeContentItem;
        vm.openContentPicker = openContentPicker;
        vm.openFilePicker = openFilePicker;
        vm.removeFile = removeFile;
        vm.openViewPicker = openViewPicker;
        vm.removePackageView = removePackageView;
        vm.downloadFile = downloadFile;
        vm.contributorsEditor = null;

        vm.buttonLabel = "";

        const packageId = $routeParams.id;
        const create = $routeParams.create;

        function onInit() {

            if (create) {
                //pre populate package with some values
                packageResource.getEmpty().then(scaffold => {
                    vm.package = scaffold;

                    buildContributorsEditor(vm.package);

                    vm.loading = false;
                });

                localizationService.localize("general_create").then(function(value) {
                    vm.buttonLabel = value;
                });
            } else {
                // load package
                packageResource.getCreatedById(packageId).then(createdPackage => {
                    vm.package = createdPackage;

                    buildContributorsEditor(vm.package);

                    vm.loading = false;
                    // get render model for content node
                    if(vm.package.contentNodeId) {
                        entityResource.getById(vm.package.contentNodeId, "Document")
                            .then((entity) => {
                                vm.contentNodeDisplayModel = entity;
                            });
                    }

                });

                localizationService.localize("buttons_save").then(function (value) {
                    vm.buttonLabel = value;
                });
            }

            // get all doc types
            entityResource.getAll("DocumentType").then(documentTypes => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                documentTypes.forEach(documentType => {
                    documentType.id = documentType.id.toString();
                });
                vm.documentTypes = documentTypes;
            });

            // get all templates
            entityResource.getAll("Template").then(templates => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                templates.forEach(template => {
                    template.id = template.id.toString();
                });
                vm.templates = templates;
            });

            // get all stylesheets
            entityResource.getAll("Stylesheet").then(stylesheets => {
                vm.stylesheets = stylesheets;
            });

            entityResource.getAll("Macro").then(macros => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                macros.forEach(macro => {
                    macro.id = macro.id.toString();
                });
                vm.macros = macros;
            });

            // get all languages
            entityResource.getAll("Language").then(languages => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                languages.forEach(language => {
                    language.id = language.id.toString();
                });
                vm.languages = languages;
            });

            // get all dictionary items
            entityResource.getAll("DictionaryItem").then(dictionaryItems => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                dictionaryItems.forEach(dictionaryItem => {
                    dictionaryItem.id = dictionaryItem.id.toString();
                });
                vm.dictionaryItems = dictionaryItems;
            });

            // get all data types items
            entityResource.getAll("DataType").then(dataTypes => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                dataTypes.forEach(dataType => {
                    dataType.id = dataType.id.toString();
                });
                vm.dataTypes = dataTypes;
            });

        }

        function downloadFile(id) {
            var url = umbRequestHelper.getApiUrl(
                "packageApiBaseUrl",
                "DownloadCreatedPackage",
                { id: id });

            umbRequestHelper.downloadFile(url).then(function () {

            });
        }

        function back() {
            $location.path("packages/packages/created").search("create", null).search("packageId", null);
        }

        function createOrUpdatePackage(editPackageForm) {

            let contributors = vm.contributorsEditor.value.map(o => o.value);

            vm.package.contributors = contributors;

            if (formHelper.submitForm({ formCtrl: editPackageForm, scope: $scope })) {

                vm.buttonState = "busy";

                packageResource.savePackage(vm.package).then((updatedPackage) => {

                    vm.package = updatedPackage;
                    vm.buttonState = "success";

                    formHelper.resetForm({ scope: $scope });

                    if (create) {
                        //if we are creating, then redirect to the correct url and reload
                        $location.path("packages/packages/edit/" + vm.package.id).search("create", null);
                        //don't add a browser history for this
                        $location.replace();
                    }
                    
                }, function(err){
                    formHelper.handleError(err);
                    vm.buttonState = "error";
                });
            }
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
                isDialog: true,
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

        function openViewPicker() {
            const controlPicker = {
                title: "Select view",
                section: "settings",
                treeAlias: "files",
                entityType: "file",
                onlyInitialized: false,
                filter: function (i) {
                    if (i.name.indexOf(".html") === -1 &&
                        i.name.indexOf(".htm") === -1) {
                        return true;
                    }
                },
                filterCssClass: "not-allowed",
                select: function(node) {
                    const id = unescape(node.id);
                    vm.package.packageView = id;
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.treePicker(controlPicker);
        }

        function removePackageView() {
            vm.package.packageView = null;
        }

        function buildContributorsEditor(pkg) {
            
            vm.contributorsEditor = {
                alias: "contributors",
                editor: "Umbraco.MultipleTextstring",
                label: "Contributors",
                description: "",
                hideLabel: true,
                view: "views/propertyeditors/multipletextbox/multipletextbox.html",
                value: getVals(pkg.contributors),
                validation: {
                    mandatory: false,
                    pattern: null
                },
                config: {
                    min: 0,
                    max: 0
                }
            };
        }

        function getVals(array) {
            var vals = [];
            for (var i = 0; i < array.length; i++) {
                vals.push({ value: array[i] });
            }
            return vals;
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.EditController", EditController);

})();
