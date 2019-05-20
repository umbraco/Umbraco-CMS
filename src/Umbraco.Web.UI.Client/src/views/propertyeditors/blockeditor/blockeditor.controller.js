function BlockEditorPropertyEditorController($scope, contentResource, editorService) {
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

    function editContent(block) {
        var scaffold = _.findWhere(vm.scaffolds, {udi: block.udi});
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
            submit: function(model) {
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
            close: function() {
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
            submit: function(model) {
                console.log("TODO: something with block settings", model)
                editorService.close();
            },
            close: function() {
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
