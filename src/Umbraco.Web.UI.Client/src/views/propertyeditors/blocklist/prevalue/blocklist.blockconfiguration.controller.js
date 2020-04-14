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

    function BlockConfigurationController($scope, elementTypeResource, overlayService, localizationService, editorService) {

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

        vm.requestRemoveBlockByIndex = function (index) {
            localizationService.localizeMany(["general_delete", "blockEditor_confirmDeleteBlockMessage", "blockEditor_confirmDeleteBlockNotice"]).then(function (data) {
                var contentElementType = vm.getElementTypeByAlias($scope.model.value[index].contentTypeAlias);
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
                    return type.alias === entry.contentTypeAlias;
                });
            });
        };

        vm.getElementTypeByAlias = function(alias) {
            return _.find(vm.elementTypes, function (type) {
                return type.alias === alias;
            });
        };

        vm.openAddDialog = function ($event, entry) {

            //we have to add the alias to the objects (they are stored as contentTypeAlias)
            var selectedItems = _.each($scope.model.value, function (obj) {
                obj.alias = obj.contentTypeAlias;
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
                        vm.createElementTypeAndAdd(vm.addBlockFromElementTypeAlias);
                    },
                    icon: "icon-add",
                    name: "Create new"
                },
                position: "target",
                event: $event,
                size: availableItems.length < 7 ? "small" : "medium",
                submit: function (overlay) {
                    vm.addBlockFromElementTypeAlias(overlay.selectedItem.alias);
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

        vm.addBlockFromElementTypeAlias = function(alias) {

            var entry = {
                "contentTypeAlias": alias,
                "view": null,
                "labelTemplate": "",
                "settingsElementTypeAlias": null
            };

            $scope.model.value.push(entry);
        };





        vm.openBlockOverlay = function (block) {

            localizationService.localize("blockEditor_blockConfigurationOverlayTitle", [vm.getElementTypeByAlias(block.contentTypeAlias).name]).then(function (data) {

                var clonedBlockData = angular.copy(block);
                vm.openBlock = block;

                var overlayModel = {
                    block: clonedBlockData,
                    title: data,
                    view: "views/propertyeditors/blocklist/prevalue/blocklist.blockconfiguration.overlay.html",
                    size: "small",
                    submit: function(overlayModel) {
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


        
        onInit();

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockList.BlockConfigurationController", BlockConfigurationController);

})();
