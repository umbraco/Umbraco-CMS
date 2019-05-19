function BlockEditorPropertyEditorController($scope, contentResource, editorService) {
    var vm = this;

    vm.scaffolds = [];
    vm.blocks = [];
    vm.loading = true;
    vm.sortableOptions = {
        axis: "y",
        cursor: "move",
        handle: ".handle",
        tolerance: 'pointer'
    };

    vm.add = add;
    vm.edit = edit;
    vm.remove = remove;

    // it would be awesome if we could load all scaffolds in one go... however we need to have an eye out for performance,
    // oddly enough it's been shown to actually be slower to load them all at once instead of one at a time
    var scaffoldsLoaded = 0;
    function init() {
        $scope.model.value = $scope.model.value || [];
        _.each($scope.model.config.blocks, function (blockConfig) {
            contentResource.getScaffoldByUdi(-20, blockConfig.elementType).then(function (scaffold) {
                if (scaffold.isElement) {
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
        var block = angular.copy(scaffold);
        edit(block);
    }

    function edit(block) {
        var options = {
            node: block,
            title: "TODO: Edit block title here",
            view: "views/propertyeditors/blockeditor/blockeditor.editblock.html",
            submit: function(model) {
                if (vm.blocks.indexOf(block) < 0) {
                    vm.blocks.push(block);
                }
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
            vm.blocks.splice(vm.blocks.indexOf(block), 1);
        }
    }

    init();
}
angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.PropertyEditorController", BlockEditorPropertyEditorController);
