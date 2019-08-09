function BlockEditorDirectiveController($scope, contentResource, editorService, iconHelper, clipboardService) {  
    var vm = this;

    vm.scaffolds = []; 
    vm.loading = true;

    let allowedElements = []; 
     
    // it would be awesome if we could load all scaffolds in one go... however we need to have an eye out for performance,
    // oddly enough it's been shown to actually be slower to load them all at once instead of one at a time
    var scaffoldsLoaded = 0;
 
    function init() {
        if (!$scope.blocks) {
            $scope.blocks = [];
        }

        _.each($scope.config, function (config) {
            contentResource.getScaffoldByUdi(-20, config.elementType).then(function (scaffold) {
                if (scaffold.isElement) {
                    // the scaffold udi is not the same as the element type udi, but we need it to be for comparison
                    scaffold.udi = config.elementType;
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
        if ($scope.config.length === scaffoldsLoaded) {
            vm.scaffolds = _.sortBy(vm.scaffolds, function (scaffold) {
                return _.findIndex($scope.config, function (blockConfig) {
                    return blockConfig.elementType === scaffold.udi;
                });
            });

            _.each($scope.blocks, function (block) {
                applyFakeSettings(block);
            });
            
            allowedElements = vm.scaffolds.map(x => x.contentTypeAlias);
            
            vm.loading = false;
        }
    }

    
    vm.openNodeTypePicker = function($event) {
        
        vm.overlayMenu = {
            show: false,
            style: {},
            filter: vm.scaffolds.length > 12 ? true : false,
            orderBy: "$index",
            view: "itempicker",
            event: $event,
            clickPasteItem: function (item) {
                vm.pasteFromClipboard(item.data);
                vm.overlayMenu.show = false;
                vm.overlayMenu = null;
            },
            submit: function (model) {
                if (model && model.selectedItem) {                    
                    vm.add(vm.scaffolds.filter(x => x.contentTypeAlias === model.selectedItem.alias)[0]);
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
        vm.scaffolds.forEach(function (scaffold) {
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
        var availableNodesForPaste = clipboardService.retriveDataOfType("elementType", allowedElements);
        availableNodesForPaste.forEach(function (node) {
            vm.overlayMenu.pasteItems.push({
                alias: node.contentTypeAlias,
                name: node.name, //contentTypeName
                data: node,
                icon: iconHelper.convertFromLegacyIcon(node.icon)
            });
        });

        vm.overlayMenu.clickClearPaste = function ($event) {
            $event.stopPropagation();
            $event.preventDefault();
            clipboardService.clearEntriesOfType("elementType", allowedElements);
            vm.overlayMenu.pasteItems = []; // This dialog is not connected via the clipboardService events, so we need to update manually.
        };

        if (vm.overlayMenu.availableItems.length === 1 && vm.overlayMenu.pasteItems.length === 0) {
            // only one scaffold type - no need to display the picker
            vm.add(vm.scaffolds[0]);
            return;
        }

        vm.overlayMenu.show = true;
    } 
    
     vm.add = function(scaffold) {
        var element = angular.copy(scaffold);
        var block = {
            udi: element.udi,
            icon: element.icon, 
            description: element.documentType.description, // probably don't want to persist these.
            content: {},
            settings: {
                view: 'views/propertyeditors/blockeditor/blockeditor.block.html'
            }  
        };
        vm.openContent(element, block);
    }

    vm.editContent = function(block) {
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

        vm.openContent(element, block);
    }

    vm.openContent = function(element, block) {
        var options = {
            element: element,
            title: 'Edit block',
            view: "views/propertyeditors/blockeditor/blockeditor.editcontent.html",
            submit: function (model) {
                _.each(element.variants[0].tabs, function (tab) {
                    _.each(tab.properties, function (property) {
                        block.content[property.alias] = property.value;
                    });
                });
                if ($scope.blocks.indexOf(block) < 0) {
                    $scope.blocks.push(block);
                    applyFakeSettings(block);
                }
                
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };
        editorService.open(options);
    }

    vm.editSettings = function(block) {
        var options = {
            settings: block.settings,
            title: "Edit settings",
            view: "views/propertyeditors/blockeditor/blockeditor.editsettings.html",
            size: "small",
            submit: function(model) {
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };
        editorService.open(options);
    }

    vm.remove = function(block) {
        // this should be replaced by a custom dialog (pending some PRs)
        if (confirm("TODO: Are you sure?")) {
            $scope.blocks.splice($scope.blocks.indexOf(block), 1);
        }
    }
    
        // TODO: remove this (only for testing)
    // currently pushes random values for setting a css grid location
    function applyFakeSettings(block) {
        block.settings["col"] = 1 + Math.floor(Math.random() * 7);
        block.settings["row"] = 1 + Math.floor(Math.random() * 7);
        block.settings["w"] = 1 + Math.floor(Math.random() * 3);
        block.settings["h"] = 1 + Math.floor(Math.random() * 3);
    }
    
    init();
}
angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.DirectiveController", ['$scope', 'contentResource', 'editorService', 'iconHelper', 'clipboardService', BlockEditorDirectiveController]);
