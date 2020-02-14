/**
 * @ngdoc controller
 * @name Umbraco.Editors.PropertySettingsController
 * @function
 *
 * @description
 * The controller for the content type editor property settings dialog
 */

(function () {
    "use strict";

    function ElementTypePickerController($scope, elementTypeResource, overlayService, localizationService, editorService) {

        var vm = this;

        vm.enableAddEntry = true;

        function evaluateStatus() {

            if (!vm.elementTypes) return;// cancel if elementTypes isnt loaded jet.

            vm.enableAddEntry = vm.getAvailableElementTypes().length > 0;

        }

        function onInit() {

            if (!$scope.model.value) {
                $scope.model.value = [];
            }

            localizationService.localize("content_nestedContentSelectElementTypeModalTitle").then(function (value) {
                //selectElementTypeModalTitle = value;
            });

            loadElementTypes();

        }

        function loadElementTypes() {
            return elementTypeResource.getAll().then(function (elementTypes) {
                vm.elementTypes = elementTypes;
                evaluateStatus();
            });
        }

        vm.requestRemoveEntryByIndex = function (index) {
            localizationService.localizeMany(["general_delete", "blockEditor_confirmDeleteBlockMessage", "blockEditor_confirmDeleteBlockNotice"]).then(function (data) {
                var contentElementType = vm.getElementTypeByAlias($scope.model.value[index].elementTypeAlias);
                overlayService.confirmDelete({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [contentElementType.name]),
                    confirmMessage: data[2],
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.removeEntryByIndex(index);
                        overlayService.close();
                    }
                });
            });
        }

        vm.removeEntryByIndex = function (index) {
            $scope.model.value.splice(index, 1);
        };
        
        vm.sortableOptions = {
            axis: "y",
            cursor: "grabbing",
            placeholder: 'sortable-placeholder',
            forcePlaceholderSize: true
        };
        

        vm.getAvailableElementTypes = function () {
            return vm.elementTypes.filter(function (type) {
                return !$scope.model.value.find(function (entry) {
                    return type.alias === entry.elementTypeAlias;
                });
            });
        };

        vm.getElementTypeByAlias = function(alias) {
            return _.find(vm.elementTypes, function (type) {
                return type.alias === alias;
            });
        };

        vm.openAddDialog = function ($event, entry) {

            //we have to add the alias to the objects (they are stored as elementTypeAlias)
            var selectedItems = _.each($scope.model.value, function (obj) {
                obj.alias = obj.elementTypeAlias;
                return obj;
            });

            var availableItems = vm.getAvailableElementTypes()

            var elemTypeSelectorOverlay = {
                view: "itempicker",
                title: "no title jet",
                availableItems: availableItems,
                selectedItems: selectedItems,
                createNewItem: {
                    action: function() {
                        overlayService.close();
                        vm.createElementTypeAndAdd(vm.addEntryFromElementTypeAlias);
                    },
                    icon: "icon-add",
                    name: "Create new"
                },
                position: "target",
                event: $event,
                size: availableItems.length < 7 ? "small" : "medium",
                submit: function (overlay) {
                    vm.addEntryFromElementTypeAlias(overlay.selectedItem.alias);
                    overlayService.close();
                },
                close: function () {
                    overlayService.close();
                }
            };

            overlayService.open(elemTypeSelectorOverlay);
        };

        vm.createElementTypeAndAdd = function(callback) {
            const editor = {
                create: true,
                infiniteMode: true,
                isElement: true,
                submit: function (model) {
                    console.log(model)
                    loadElementTypes().then( function () {
                        callback(model.documentTypeAlias);
                    });
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.documentTypeEditor(editor);
        }

        vm.addEntryFromElementTypeAlias = function(alias) {

            var entry = {
                "elementTypeAlias": alias,
                "view": null,
                "labelTemplate": "",
                "settingsElementTypeAlias": null
            };

            $scope.model.value.push(entry);
        };

        vm.requestRemoveSettingsForEntry = function(entry) {
            localizationService.localizeMany(["general_remove", "defaultdialogs_confirmremoveusageof"]).then(function (data) {

                var settingsElementType = vm.getElementTypeByAlias(entry.settingsElementTypeAlias);

                overlayService.confirmRemove({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [settingsElementType.name]),
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.removeSettingsForEntry(entry);
                        overlayService.close();
                    }
                });
            });
        };
        vm.removeSettingsForEntry = function(entry) {
            entry.settingsElementTypeAlias = null;
        };

        vm.addSettingsForEntry = function ($event, entry) {

            var elemTypeSelectorOverlay = {
                view: "itempicker",
                title: "Pick settings (missing translation)",
                availableItems: vm.elementTypes,
                position: "target",
                event: $event,
                size: vm.elementTypes.length < 7 ? "small" : "medium",
                createNewItem: {
                    action: function() {
                        overlayService.close();
                        vm.createElementTypeAndAdd((alias) => {
                            vm.addSettingsAtEntry(entry, alias);
                        });
                    },
                    icon: "icon-add",
                    name: "Create new"
                },
                submit: function (overlay) {
                    vm.addSettingsAtEntry(entry, overlay.selectedItem.alias);
                    overlayService.close();
                },
                close: function () {
                    overlayService.close();
                }
            };

            overlayService.open(elemTypeSelectorOverlay);
        };
        vm.addSettingsAtEntry = function(entry, alias) {
            entry.settingsElementTypeAlias = alias;
        };

        vm.openElementType = function(elementTypeAlias) {
            var elementTypeId = vm.getElementTypeByAlias(elementTypeAlias).id;
            const editor = {
                id: elementTypeId,
                submit: function (model) {
                    loadElementTypes();
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.documentTypeEditor(editor);
        }

        vm.requestRemoveViewForEntry = function(entry) {
            localizationService.localizeMany(["general_remove", "defaultdialogs_confirmremoveusageof"]).then(function (data) {
                overlayService.confirmRemove({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [entry.view]),
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.removeViewForEntry(entry);
                        overlayService.close();
                    }
                });
            });
        };
        vm.removeViewForEntry = function(entry) {
            entry.view = null;
        };
        vm.addViewForEntry = function(entry) {
            const filePicker = {
                title: "Select view (TODO need translation)",
                section: "settings",
                treeAlias: "files",
                entityType: "file",
                isDialog: true,
                filter: function (i) {
                    if (i.name.indexOf(".html") !== -1) {
                        return true;
                    }
                },
                select: function (file) {
                    console.log(file);
                    entry.view = file.name;
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.treePicker(filePicker);
        }

        
        onInit();

        $scope.$watchCollection('model.value', function(newVal, oldVal) {
            evaluateStatus();
        });

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockList.ElementTypePickerController", ElementTypePickerController);

})();
