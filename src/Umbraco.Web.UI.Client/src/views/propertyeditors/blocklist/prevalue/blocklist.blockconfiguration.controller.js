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

    function BlockConfigurationController($scope, elementTypeResource, overlayService, localizationService, editorService, eventsService, udiService) {

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
                overlayService.confirmDelete({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [contentElementType ? contentElementType.name : "(Unavailable ElementType)"]),
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
                }) || null;
            }
        };

        vm.openAddDialog = function () {

            localizationService.localize("blockEditor_headlineCreateBlock").then(function(localizedTitle) {

                const contentTypePicker = {
                    title: localizedTitle,
                    section: "settings",
                    treeAlias: "documentTypes",
                    entityType: "documentType",
                    isDialog: true,
                    filter: function (node) {
                        if (node.metaData.isElement === true) {
                            var key = udiService.getKey(node.udi);
                            // If a Block with this ElementType as content already exists, we will emit it as a posible option.
                            return $scope.model.value.find(function (entry) {
                                return key === entry.contentElementTypeKey;
                            });
                        }
                        return true;
                    },
                    filterCssClass: "not-allowed",
                    select: function (node) {
                        vm.addBlockFromElementTypeKey(udiService.getKey(node.udi));
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    },
                    extraActions: [
                        {
                            style: "primary",
                            labelKey: "blockEditor_labelcreateNewElementType",
                            action: function () {
                                vm.createElementTypeAndCallback((documentTypeKey) => {
                                    vm.addBlockFromElementTypeKey(documentTypeKey);

                                    // At this point we will close the contentTypePicker.
                                    editorService.close();
                                });
                            }
                        }
                    ]
                };
                editorService.treePicker(contentTypePicker);

            });
        };

        vm.createElementTypeAndCallback = function(callback) {
            const editor = {
                create: true,
                infiniteMode: true,
                noTemplate: true,
                isElement: true,
                noTemplate: true,
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

            var blockType = {
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

            $scope.model.value.push(blockType);

            vm.openBlockOverlay(blockType);

        };





        vm.openBlockOverlay = function (block) {

            var elementType = vm.getElementTypeByKey(block.contentElementTypeKey);

            if(elementType) {
                localizationService.localize("blockEditor_blockConfigurationOverlayTitle", [elementType.name]).then(function (data) {

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
            } else {
                alert("Cannot be edited cause ElementType does not exist.");
            }

        };

        $scope.$on('$destroy', function () {
            unsubscribe.forEach(u => { u(); });
        });

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockList.BlockConfigurationController", BlockConfigurationController);

})();
