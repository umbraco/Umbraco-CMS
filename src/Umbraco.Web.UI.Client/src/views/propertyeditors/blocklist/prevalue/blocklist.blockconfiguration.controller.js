/**
 * @ngdoc controller
 * @name Umbraco.Editors.BlockList.BlockConfigurationController
 * @function
 *
 * @description
 * The controller for the content type editor property settings dialog
 */

(function () {
    "use strict";

    function TransferProperties(fromObject, toObject) {
        for (var p in fromObject) {
            toObject[p] = fromObject[p];
        }
    }

    function BlockConfigurationController($scope, elementTypeResource, overlayService, localizationService, editorService, eventsService) {

        var unsubscribe = [];

        var vm = this;
        vm.openBlock = null;

        function onInit() {

            if (!$scope.model.value) {
                $scope.model.value = [];
            }

            loadElementTypes();

        }

        function loadElementTypes() {
            return elementTypeResource.getAll().then(function (elementTypes) {
                vm.elementTypes = elementTypes;
            });
        }

        function updateUsedElementTypes(event, args) {
            var key = args.documentType.key;
            for (var i = 0; i<vm.elementTypes.length; i++) {
                if (vm.elementTypes[i].key === key) {
                    vm.elementTypes[i] = args.documentType;
                }
            }
        }
        unsubscribe.push(eventsService.on("editors.documentType.saved", updateUsedElementTypes));

        vm.requestRemoveBlockByIndex = function (index) {
            localizationService.localizeMany(["general_delete", "blockEditor_confirmDeleteBlockTypeMessage", "blockEditor_confirmDeleteBlockTypeNotice"]).then(function (data) {
                var contentElementType = vm.getElementTypeByKey($scope.model.value[index].contentElementTypeKey);
                if(contentElementType == null) {
                    contentElementType = {
                        name: "Unavailable ElementType"
                    }
                }
                overlayService.confirmDelete({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [contentElementType.name]),
                    confirmMessage: data[2],
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.removeBlockByIndex(index);
                        overlayService.close();
                    }
                });
            });
        }

        vm.removeBlockByIndex = function (index) {
            $scope.model.value.splice(index, 1);
        };

        vm.sortableOptions = {
            "ui-floating": true,
            items: "umb-block-card",
            cursor: "grabbing",
            placeholder: 'umb-block-card --sortable-placeholder'
        };


        vm.getAvailableElementTypes = function () {
            return vm.elementTypes.filter(function (type) {
                return !$scope.model.value.find(function (entry) {
                    return type.key === entry.contentElementTypeKey;
                });
            });
        };

        vm.getElementTypeByKey = function(key) {
            if (vm.elementTypes) {
                return vm.elementTypes.find(function (type) {
                    return type.key === key;
                });
            }
        };

        vm.openAddDialog = function ($event, entry) {

            //we have to add the 'alias' property to the objects, to meet the data requirements of itempicker.
            var selectedItems = Utilities.copy($scope.model.value).forEach((obj) => {
                obj.alias = vm.getElementTypeByKey(obj.contentElementTypeKey).alias;
                return obj;
            });

            var availableItems = vm.getAvailableElementTypes()

            localizationService.localizeMany(["blockEditor_headlineCreateBlock", "blockEditor_labelcreateNewElementType"]).then(function(localized) {

                var elemTypeSelectorOverlay = {
                    view: "itempicker",
                    title: localized[0],
                    availableItems: availableItems,
                    selectedItems: selectedItems,
                    createNewItem: {
                        action: function() {
                            overlayService.close();
                            vm.createElementTypeAndCallback(vm.addBlockFromElementTypeKey);
                        },
                        icon: "icon-add",
                        name: localized[1]
                    },
                    position: "target",
                    event: $event,
                    size: availableItems.length < 7 ? "small" : "medium",
                    submit: function (overlay) {
                        vm.addBlockFromElementTypeKey(overlay.selectedItem.key);
                        overlayService.close();
                    },
                    close: function () {
                        overlayService.close();
                    }
                };

                overlayService.open(elemTypeSelectorOverlay);

            });
        };

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

        vm.addBlockFromElementTypeKey = function(key) {

            var entry = {
                "contentElementTypeKey": key,
                "settingsElementTypeKey": null,
                "labelTemplate": "",
                "view": null,
                "stylesheet": null,
                "editorSize": "medium",
                "iconColor": null,
                "backgroundColor": null,
                "thumbnail": null
            };

            $scope.model.value.push(entry);
        };





        vm.openBlockOverlay = function (block) {

            localizationService.localize("blockEditor_blockConfigurationOverlayTitle", [vm.getElementTypeByKey(block.contentElementTypeKey).name]).then(function (data) {

                var clonedBlockData = Utilities.copy(block);
                vm.openBlock = block;

                var overlayModel = {
                    block: clonedBlockData,
                    title: data,
                    view: "views/propertyeditors/blocklist/prevalue/blocklist.blockconfiguration.overlay.html",
                    size: "small",
                    submit: function(overlayModel) {
                        loadElementTypes()// lets load elementType again, to ensure we are up to date.
                        TransferProperties(overlayModel.block, block);// transfer properties back to block object. (Doing this cause we dont know if block object is added to model jet, therefor we cant use index or replace the object.)
                        overlayModel.close();
                    },
                    close: function() {
                        editorService.close();
                        vm.openBlock = null;
                    }
                };

                // open property settings editor
                editorService.open(overlayModel);

            });

        };

        $scope.$on('$destroy', function () {
            unsubscribe.forEach(u => { u(); });
        });

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockList.BlockConfigurationController", BlockConfigurationController);

})();
