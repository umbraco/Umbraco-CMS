/**
 * @ngdoc controller
 * @name Umbraco.Editors.BlockList.BlockConfigurationOverlayController
 * @function
 *
 * @description
 * The controller for the content type editor property settings dialog
 */

(function () {
    "use strict";

    function BlockConfigurationOverlayController($scope, overlayService, localizationService, editorService, elementTypeResource) {

        var vm = this;
        vm.block = $scope.model.block;

        loadElementTypes();

        function loadElementTypes() {
            return elementTypeResource.getAll().then(function (elementTypes) {
                vm.elementTypes = elementTypes;
            });
        }

        vm.getElementTypeByKey = function(key) {
            return _.find(vm.elementTypes, function (type) {
                return type.key === key;
            });
        };

        vm.openElementType = function(elementTypeKey) {
            var elementTypeId = vm.getElementTypeByKey(elementTypeKey).id;
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

        vm.createElementTypeAndCallback = function(callback) {
            const editor = {
                create: true,
                infiniteMode: true,
                isElement: true,
                submit: function (model) {
                    loadElementTypes().then( function () {
                        callback(model.documentTypeKey);
                    });
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.documentTypeEditor(editor);
        }

        vm.addSettingsForBlock = function ($event, block) {

            localizationService.localizeMany(["blockEditor_headlineAddSettingsElementType", "blockEditor_labelcreateNewElementType"]).then(function(localized) {

                var elemTypeSelectorOverlay = {
                    view: "itempicker",
                    title: localized[0],
                    availableItems: vm.elementTypes,
                    position: "target",
                    event: $event,
                    size: vm.elementTypes.length < 7 ? "small" : "medium",
                    createNewItem: {
                        action: function() {
                            overlayService.close();
                            vm.createElementTypeAndCallback((key) => {
                                vm.applySettingsToBlock(block, key);
                            });
                        },
                        icon: "icon-add",
                        name: localized[1]
                    },
                    submit: function (overlay) {
                        vm.applySettingsToBlock(block, overlay.selectedItem.key);
                        overlayService.close();
                    },
                    close: function () {
                        overlayService.close();
                    }
                };

                overlayService.open(elemTypeSelectorOverlay);

            });
        };
        vm.applySettingsToBlock = function(block, key) {
            block.settingsElementTypeKey = key;
        };

        vm.requestRemoveSettingsForBlock = function(block) {
            localizationService.localizeMany(["general_remove", "defaultdialogs_confirmremoveusageof"]).then(function (data) {

                var settingsElementType = vm.getElementTypeByKey(block.settingsElementTypeKey);

                overlayService.confirmRemove({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [settingsElementType.name]),
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.removeSettingsForBlock(block);
                        overlayService.close();
                    }
                });
            });
        };
        vm.removeSettingsForBlock = function(block) {
            block.settingsElementTypeKey = null;
        };


        vm.addViewForBlock = function(block) {
            localizationService.localize("blockEditor_headlineSelectView").then(function(localizedTitle) {

                const filePicker = {
                    title: localizedTitle,
                    section: "settings",
                    treeAlias: "files",
                    entityType: "file",
                    isDialog: true,
                    select: function (node) {
                        console.log(node)
                        const filepath = decodeURIComponent(node.id.replace(/\+/g, " "));
                        block.view = filepath;
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.treePicker(filePicker);
            
            });
        }
        vm.requestRemoveViewForBlock = function(block) {
            localizationService.localizeMany(["general_remove", "defaultdialogs_confirmremoveusageof"]).then(function (data) {
                overlayService.confirmRemove({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [block.view]),
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.removeViewForBlock(block);
                        overlayService.close();
                    }
                });
            });
        };
        vm.removeViewForBlock = function(block) {
            block.view = null;
        };


        
        vm.addStylesheetForBlock = function(block) {
            localizationService.localize("blockEditor_headlineAddCustomStylesheet").then(function(localizedTitle) {
                    
                const filePicker = {
                    title: localizedTitle,
                    section: "settings",
                    treeAlias: "files",
                    entityType: "file",
                    isDialog: true,
                    select: function (node) {
                        const filepath = decodeURIComponent(node.id.replace(/\+/g, " "));
                        block.stylesheet = filepath;
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.treePicker(filePicker);

            });
        }
        vm.requestRemoveStylesheetForBlock = function(block) {
            localizationService.localizeMany(["general_remove", "defaultdialogs_confirmremoveusageof"]).then(function (data) {
                overlayService.confirmRemove({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [block.stylesheet]),
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.removeStylesheetForBlock(block);
                        overlayService.close();
                    }
                });
            });
        };
        vm.removeStylesheetForBlock = function(block) {
            block.stylesheet = null;
        };



        vm.addThumbnailForBlock = function(block) {

            localizationService.localize("blockEditor_headlineAddThumbnail").then(function(localizedTitle) {

                const thumbnailPicker = {
                    title: localizedTitle,
                    section: "settings",
                    treeAlias: "files",
                    entityType: "file",
                    isDialog: true,
                    filter: function (i) {
                        return !(i.name.indexOf(".jpg") !== -1 || i.name.indexOf(".jpeg") !== -1 || i.name.indexOf(".png") !== -1 || i.name.indexOf(".svg") !== -1 || i.name.indexOf(".webp") !== -1 || i.name.indexOf(".gif") !== -1);
                    },
                    select: function (file) {
                        block.thumbnail = file.name;
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.treePicker(thumbnailPicker);

            });
        }
        vm.removeThumbnailForBlock = function(entry) {
            entry.thumbnail = null;
        };




        vm.submit = function () {
            if ($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }

        vm.close = function() {
            if ($scope.model && $scope.model.close) {
                // TODO: If content has changed, we should notify user.
                $scope.model.close($scope.model);
            }
        }

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockList.BlockConfigurationOverlayController", BlockConfigurationOverlayController);

})();
