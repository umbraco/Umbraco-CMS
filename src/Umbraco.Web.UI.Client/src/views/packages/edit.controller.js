(function () {
    "use strict";

    function EditController($scope, $location, $routeParams, umbRequestHelper, entityResource, packageResource, editorService, formHelper, localizationService) {

        const vm = this;

        const packageId = $routeParams.id;
        const create = $routeParams.create;

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

        vm.selectDocumentType = selectDocumentType;
        vm.selectTemplate = selectTemplate;
        vm.selectStyleSheet = selectStyleSheet;
        vm.selectMacro = selectMacro;
        vm.selectLanguage = selectLanguage;
        vm.selectDictionaryItem = selectDictionaryItem;
        vm.selectDataType = selectDataType;

        vm.labels = {};

        vm.versionRegex = /^(\d+\.)(\d+\.)(\*|\d+)$/;

        vm.aceOption = {
            mode: "xml",
            theme: "chrome",
            showPrintMargin: false,
            advanced: {
                fontSize: '14px',
                enableSnippets: true,
                enableBasicAutocompletion: true,
                enableLiveAutocompletion: false
            },
            onLoad: function (_editor) {
                vm.editor = _editor;

                vm.editor.setValue(vm.package.actions);
            }
        };

        function onInit() {

            if (create) {
                // Pre populate package with some values
                packageResource.getEmpty().then(scaffold => {
                    vm.package = scaffold;

                    loadResources();

                    buildContributorsEditor(vm.package);

                    vm.loading = false;
                });

                localizationService.localizeMany(["general_create", "packager_includeAllChildNodes"]).then(function (values) {
                    vm.labels.button = values[0];
                    vm.labels.includeAllChildNodes = values[1];
                });
            } else {
                // Load package
                packageResource.getCreatedById(packageId).then(createdPackage => {
                    vm.package = createdPackage;

                    loadResources();

                    buildContributorsEditor(vm.package);

                    vm.loading = false;

                    // Get render model for content node
                    if (vm.package.contentNodeId) {
                        entityResource.getById(vm.package.contentNodeId, "Document")
                            .then((entity) => {
                                vm.contentNodeDisplayModel = entity;
                            });
                    }

                });


                localizationService.localizeMany(["buttons_save", "packager_includeAllChildNodes"]).then(function (values) {
                    vm.labels.button = values[0];
                    vm.labels.includeAllChildNodes = values[1];
                });
            }
        }

        function loadResources() {

            // Get all document types
            entityResource.getAll("DocumentType").then(documentTypes => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                documentTypes.forEach(documentType => {
                    documentType.id = documentType.id.toString();
                    documentType.selected = vm.package.documentTypes.indexOf(documentType.id) !== -1;
                });
                vm.documentTypes = documentTypes;
            });

            // Get all templates
            entityResource.getAll("Template").then(templates => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                templates.forEach(template => {
                    template.id = template.id.toString();
                    template.selected = vm.package.templates.indexOf(template.id) >= 0;
                });
                vm.templates = templates;
            });

            // Get all stylesheets
            entityResource.getAll("Stylesheet").then(stylesheets => {
                stylesheets.forEach(stylesheet => {
                    stylesheet.selected = vm.package.stylesheets.indexOf(stylesheet.name) >= 0;
                });
                vm.stylesheets = stylesheets;
            });

            // Get all macros
            entityResource.getAll("Macro").then(macros => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                macros.forEach(macro => {
                    macro.id = macro.id.toString();
                    macro.selected = vm.package.macros.indexOf(macro.id) !== -1;
                });
                vm.macros = macros;
            });

            // Get all languages
            entityResource.getAll("Language").then(languages => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                languages.forEach(language => {
                    language.id = language.id.toString();
                    language.selected = vm.package.languages.indexOf(language.id) !== -1;
                });
                vm.languages = languages;
            });

            // Get all dictionary items
            entityResource.getAll("DictionaryItem").then(dictionaryItems => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                dictionaryItems.forEach(dictionaryItem => {
                    dictionaryItem.id = dictionaryItem.id.toString();
                    dictionaryItem.selected = vm.package.dictionaryItems.indexOf(dictionaryItem.id) !== -1;
                });
                vm.dictionaryItems = dictionaryItems;
            });

            // Get all data types
            entityResource.getAll("DataType").then(dataTypes => {
                // a package stores the id as a string so we 
                // need to convert all ids to string for comparison
                dataTypes.forEach(dataType => {
                    dataType.id = dataType.id.toString();
                    dataType.selected = vm.package.dataTypes.indexOf(dataType.id) !== -1;
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

                    formHelper.resetForm({ scope: $scope, formCtrl: editPackageForm });

                    if (create) {
                        //if we are creating, then redirect to the correct url and reload
                        $location.path("packages/packages/edit/" + vm.package.id).search("create", null);
                        //don't add a browser history for this
                        $location.replace();
                    }

                }, function (err) {
                    formHelper.resetForm({ scope: $scope, formCtrl: editPackageForm, hasErrors: true });
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
                submit: function (model) {
                    if (model.selection && model.selection.length > 0) {
                        vm.package.contentNodeId = model.selection[0].id.toString();
                        vm.contentNodeDisplayModel = model.selection[0];
                    }
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.contentPicker(contentPicker);
        }

        function openFilePicker() {

            let selection = Utilities.copy(vm.package.files);

            const filePicker = {
                title: "Select files",
                section: "settings",
                treeAlias: "files",
                entityType: "file",
                multiPicker: true,
                isDialog: true,
                select: function (node) {
                    node.selected = !node.selected;

                    const id = decodeURIComponent(node.id.replace(/\+/g, " "));
                    const index = selection.indexOf(id);

                    if (node.selected) {
                        if (index === -1) {
                            selection.push(id);
                        }
                    } else {
                        selection.splice(index, 1);
                    }
                },
                submit: function () {
                    vm.package.files = selection;
                    editorService.close();
                },
                close: function () {
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
                select: function (node) {
                    const id = decodeURIComponent(node.id.replace(/\+/g, " "));
                    vm.package.packageView = id;
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.treePicker(controlPicker);
        }

        function removePackageView() {
            vm.package.packageView = null;
        }

        function selectDocumentType(doctype) {

            // Check if the document type is already selected.
            var index = vm.package.documentTypes.indexOf(doctype.id);

            if (index === -1) {
                vm.package.documentTypes.push(doctype.id);
            } else {
                vm.package.documentTypes.splice(index, 1);
            }
        }

        function selectTemplate(template) {

            // Check if the template is already selected.
            var index = vm.package.templates.indexOf(template.id);

            if (index === -1) {
                vm.package.templates.push(template.id);
            } else {
                vm.package.templates.splice(index, 1);
            }
        }

        function selectStyleSheet(stylesheet) {

            // Check if the style sheet is already selected.
            var index = vm.package.stylesheets.indexOf(stylesheet.name);

            if (index === -1) {
                vm.package.stylesheets.push(stylesheet.name);
            } else {
                vm.package.stylesheets.splice(index, 1);
            }
        }

        function selectMacro(macro) {

            // Check if the macro is already selected.
            var index = vm.package.macros.indexOf(macro.id);

            if (index === -1) {
                vm.package.macros.push(macro.id);
            } else {
                vm.package.macros.splice(index, 1);
            }
        }

        function selectLanguage(language) {

            // Check if the language is already selected.
            var index = vm.package.languages.indexOf(language.id);

            if (index === -1) {
                vm.package.languages.push(language.id);
            } else {
                vm.package.languages.splice(index, 1);
            }
        }

        function selectDictionaryItem(dictionaryItem) {

            // Check if the dictionary item is already selected.
            var index = vm.package.dictionaryItems.indexOf(dictionaryItem.id);

            if (index === -1) {
                vm.package.dictionaryItems.push(dictionaryItem.id);
            } else {
                vm.package.dictionaryItems.splice(index, 1);
            }
        }

        function selectDataType(dataType) {

            // Check if the dictionary item is already selected.
            var index = vm.package.dataTypes.indexOf(dataType.id);

            if (index === -1) {
                vm.package.dataTypes.push(dataType.id);
            } else {
                vm.package.dataTypes.splice(index, 1);
            }
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
