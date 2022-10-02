/**
 * @ngdoc controller
 * @name Umbraco.Editors.BlockGrid.BlockConfigurationController
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

    const DEFAULT_GRID_COLUMNS = 12;

    function BlockConfigurationController($scope, $element, elementTypeResource, overlayService, localizationService, editorService, eventsService, udiService) {

        var unsubscribe = [];

        var vm = this;
        vm.openBlock = null;

        function onInit() {

            $element.closest('.umb-control-group').addClass('-no-border');

            // Somehow the preValues models are different, so we will try to match either key or alias.
            vm.gridColumnsPreValue = $scope.preValues.find(x => x.key ? x.key === "gridColumns" : x.alias === "gridColumns");
            const blockGroupModel = $scope.preValues.find(x => x.key ? x.key === "blockGroups" : x.alias === "blockGroups");
            if (blockGroupModel.value == null) {
                blockGroupModel.value = [];
            }
            vm.blockGroups = blockGroupModel.value;

            if (!$scope.model.value) {
                $scope.model.value = [];
            }

            // Ensure good values:
            $scope.model.value.forEach(block => {
                block.columnSpanOptions = block.columnSpanOptions || [];
            });
            $scope.model.value.forEach(block => {
                block.areas = block.areas || [];
            });

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

        function removeReferencesToElementTypeKey(contentElementTypeKey) {
            // Clean up references to this one:
            $scope.model.value.forEach(blockType => {
                blockType.areas.forEach(area => {
                    area.specifiedAllowance = area.specifiedAllowance?.filter(allowance => 
                        allowance.elementTypeKey !== contentElementTypeKey
                    ) || [];
                });
            });
        }

        function removeReferencesToGroupKey(groupKey) {
            // Clean up references to this one:
            $scope.model.value.forEach(blockType => {
                blockType.areas.forEach(area => {
                    area.specifiedAllowance = area.specifiedAllowance?.filter(allowance => 
                        allowance.groupKey !== groupKey
                    ) || [];
                });
            });
        }

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
            const blockType = $scope.model.value[index];
            if(blockType) {
                $scope.model.value.splice(index, 1);
                removeReferencesToElementTypeKey(blockType.contentElementTypeKey);
            }
        };

        const defaultOptions = {
            axis: '',
            tolerance: "pointer",
            opacity: 0.7,
            scroll: true
        };
        vm.groupSortableOptions = {
            ...defaultOptions, 
            handle: '.__handle',
            items: ".umb-block-card-group",
            cursor: "grabbing",
            placeholder: 'umb-block-card-group --sortable-placeholder'
        };
        vm.blockSortableOptions = {
            ...defaultOptions, 
            "ui-floating": true,
            connectWith: ".umb-block-card-grid",
            items: "umb-block-card",
            cursor: "grabbing",
            placeholder: '--sortable-placeholder',
            forcePlaceHolderSize: true,
            stop: function(e, ui) {
                if(ui.item.sortable.droptarget && ui.item.sortable.droptarget.length > 0) {
                    // We do not want sortable to actually move the data, as we are using the same ng-model. Instead we just change the groupKey and cancel the transfering.
                    ui.item.sortable.model.groupKey = ui.item.sortable.droptarget[0].dataset.groupKey || null;
                    ui.item.sortable.cancel();
                }
            }
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

        vm.openAddDialog = function (groupKey) {

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
                            // If a Block with this ElementType as content already exists, we will emit it as a possible option.
                            return $scope.model.value.find(function (entry) {
                                return key === entry.contentElementTypeKey;
                            });
                        }
                        return true;
                    },
                    filterCssClass: "not-allowed",
                    select: function (node) {
                        vm.addBlockFromElementTypeKey(udiService.getKey(node.udi), groupKey);
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
                                    vm.addBlockFromElementTypeKey(documentTypeKey, groupKey);

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

        vm.addBlockFromElementTypeKey = function(key, groupKey) {

            var blockType = {
                "columnSpanOptions": [],
                "allowAtRoot": true,
                "rowMinSpan": 1,
                "rowMaxSpan": 1,
                "contentElementTypeKey": key,
                "settingsElementTypeKey": null,
                "labelTemplate": "",
                "view": null,
                "stylesheet": null,
                "editorSize": "medium",
                "iconColor": null,
                "backgroundColor": null,
                "thumbnail": null,
                "areaGridColumns": null,
                "areas": [],
                "groupKey": groupKey || null
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
                        allBlockTypes: $scope.model.value,
                        allBlockGroups: vm.blockGroups,
                        loadedElementTypes: vm.elementTypes,
                        gridColumns: vm.gridColumnsPreValue.value || DEFAULT_GRID_COLUMNS,
                        title: data,
                        view: "views/propertyeditors/blockgrid/prevalue/blockgrid.blockconfiguration.overlay.html",
                        size: "large",
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


        vm.requestRemoveGroup = function(blockGroup) {
            if(blockGroup.key) {
                localizationService.localizeMany(["general_delete", "blockEditor_confirmDeleteBlockGroupMessage", "blockEditor_confirmDeleteBlockGroupNotice"]).then(function (data) {
                    overlayService.confirmDelete({
                        title: data[0],
                        content: localizationService.tokenReplace(data[1], [blockGroup.name ? blockGroup.name : "'Unnamed group'"]),
                        confirmMessage: data[2],
                        close: function () {
                            overlayService.close();
                        },
                        submit: function () {

                            // Remove all blocks of this group:
                            $scope.model.value = $scope.model.value.filter(block => {
                                    if (block.groupKey === blockGroup.key) {
                                        // Clean up references to this one:
                                        removeReferencesToElementTypeKey(block.contentElementTypeKey);

                                        return false;
                                    } else {
                                        return true;
                                    }
                                }
                            );

                            // Remove any special allowance for this

                            // Then remove group:
                            const groupIndex = vm.blockGroups.indexOf(blockGroup);
                            if(groupIndex !== -1) {
                                vm.blockGroups.splice(groupIndex, 1);
                                removeReferencesToGroupKey(blockGroup.key);
                            }

                            // Remove any special allowance for this.

                            overlayService.close();
                        }
                    });
                });
            }
        }

        $scope.$on('$destroy', function () {
            unsubscribe.forEach(u => { u(); });
        });

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockGrid.BlockConfigurationController", BlockConfigurationController);

})();
