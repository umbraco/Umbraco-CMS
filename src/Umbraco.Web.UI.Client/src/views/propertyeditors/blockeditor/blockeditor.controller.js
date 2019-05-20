function BlockEditorPropertyEditorController($scope, contentResource, editorService, iconHelper, clipboardService) {
    var vm = this;

    vm.scaffolds = [];
    vm.loading = true;
    vm.sortableOptions = {
        axis: "y",
        cursor: "move",
        handle: ".handle",
        tolerance: 'pointer'
    };

    vm.add = add;
    vm.editContent = editContent;
    vm.editSettings = editSettings;
    vm.remove = remove;
    vm.openNodeTypePicker = openNodeTypePicker;

    // it would be awesome if we could load all scaffolds in one go... however we need to have an eye out for performance,
    // oddly enough it's been shown to actually be slower to load them all at once instead of one at a time
    var scaffoldsLoaded = 0;

    function init() {
        $scope.model.value = $scope.model.value || [];
        _.each($scope.model.config.blocks, function (blockConfig) {
            contentResource.getScaffoldByUdi(-20, blockConfig.elementType).then(function (scaffold) {
                if (scaffold.isElement) {
                    // the scaffold udi is not the same as the element type udi, but we need it to be for comparison
                    scaffold.udi = blockConfig.elementType;
                    // shift this up to be consistent with other entities
                    scaffold.thumbnail = scaffold.documentType.thumbnail;
                    vm.scaffolds.push(scaffold);
                }
                scaffoldsLoaded++;
                initIfAllScaffoldsHaveLoaded();
            }, function (error) {
                scaffoldsLoaded++;
                initIfAllScaffoldsHaveLoaded();
            });
        });
    }

    function initIfAllScaffoldsHaveLoaded() {
        // Initialize when all scaffolds have loaded
        if ($scope.model.config.blocks.length === scaffoldsLoaded) {
            vm.scaffolds = _.sortBy(vm.scaffolds, function (scaffold) {
                return _.findIndex($scope.model.config.blocks, function (blockConfig) {
                    return blockConfig.elementType === scaffold.udi;
                });
            });
            vm.loading = false;
        }
    }

    function add(scaffold) {
        var element = angular.copy(scaffold);
        var block = {
            udi: element.udi,
            content: {},
            settings: {}
        };
        openContent(element, block);
    }

    // nw => this needs to be refined, is currently largely a copy from nested content
    function openNodeTypePicker($event) {
        
        vm.overlayMenu = {
            show: false,
            style: {},
            filter: vm.scaffolds.length > 12 ? true : false,
            orderBy: "$index",
            view: "itempicker",
            event: $event,
            clickPasteItem: function (item) {
                $scope.pasteFromClipboard(item.data);
                vm.overlayMenu.show = false;
                vm.overlayMenu = null;
            },
            submit: function (model) {
                if (model && model.selectedItem) {                    
                    add(vm.scaffolds.filter(x => x.contentTypeAlias === model.selectedItem.alias)[0]);
                }
                vm.overlayMenu.show = false;
                vm.overlayMenu = null;
            },
            close: function () {
                vm.overlayMenu.show = false;
                vm.overlayMenu = null;
            }
        };

        // this could be used for future limiting on node types
        vm.overlayMenu.availableItems = [];
        _.each(vm.scaffolds, function (scaffold) {
            vm.overlayMenu.availableItems.push({
                alias: scaffold.contentTypeAlias,
                name: scaffold.contentTypeName,
                icon: iconHelper.convertFromLegacyIcon(scaffold.icon),
                thumbnail: scaffold.thumbnail,
                description: scaffold.documentType.description
            });
        });

        if (vm.overlayMenu.availableItems.length === 0) {
            return;
        }

        if (vm.overlayMenu.availableItems.some(x => x.thumbnail)) {
            vm.overlayMenu.size = 'large';
        } else {
            vm.overlayMenu.size = vm.overlayMenu.availableItems.length > 6 ? "medium" : "small";
        }

        vm.overlayMenu.pasteItems = [];
//        var availableNodesForPaste = clipboardService.retriveDataOfType("elementType", contentTypeAliases);
//        _.each(availableNodesForPaste, function (node) {
//            vm.overlayMenu.pasteItems.push({
//                alias: node.contentTypeAlias,
//                name: node.name, //contentTypeName
//                data: node,
//                icon: iconHelper.convertFromLegacyIcon(node.icon)
//            });
//        });
//
//        vm.overlayMenu.title = vm.overlayMenu.pasteItems.length > 0 ? $scope.labels.grid_addElement : $scope.labels.content_createEmpty;
//
//        vm.overlayMenu.clickClearPaste = function ($event) {
//            $event.stopPropagation();
//            $event.preventDefault();
//            clipboardService.clearEntriesOfType("elementType", contentTypeAliases);
//            vm.overlayMenu.pasteItems = []; // This dialog is not connected via the clipboardService events, so we need to update manually.
//        };

        if (vm.overlayMenu.availableItems.length === 1 && vm.overlayMenu.pasteItems.length === 0) {
            // only one scaffold type - no need to display the picker
            add(vm.scaffolds[0]);
            return;
        }

        vm.overlayMenu.show = true;
    }


    function editContent(block) {
        var scaffold = _.findWhere(vm.scaffolds, {
            udi: block.udi
        });
        var element = angular.copy(scaffold);
        _.each(element.variants[0].tabs, function (tab) {
            _.each(tab.properties, function (property) {
                if (block.content[property.alias]) {
                    property.value = block.content[property.alias];
                }
            });
        });

        openContent(element, block);
    }

    function openContent(element, block) {
        var options = {
            element: element,
            title: "TODO: Edit block title here",
            view: "views/propertyeditors/blockeditor/blockeditor.editcontent.html",
            submit: function (model) {
                _.each(element.variants[0].tabs, function (tab) {
                    _.each(tab.properties, function (property) {
                        block.content[property.alias] = property.value;
                    });
                });
                if ($scope.model.value.indexOf(block) < 0) {
                    $scope.model.value.push(block);
                }

                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };
        editorService.open(options);
    }

    function editSettings(block) {
        var options = {
            settings: block.settings,
            title: "TODO: Edit settings title here",
            view: "views/propertyeditors/blockeditor/blockeditor.editsettings.html",
            size: "medium",
            submit: function (model) {
                console.log("TODO: something with block settings", model)
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };
        editorService.open(options);
    }

    function remove(block) {
        if (confirm("TODO: Are you sure?")) {
            $scope.model.value.splice($scope.model.value.indexOf(block), 1);
        }
    }

    init();
}
angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.PropertyEditorController", BlockEditorPropertyEditorController);
