function TreeSourceTypePickerController($scope, contentTypeResource, mediaTypeResource, memberTypeResource, editorService, eventsService, angularHelper) {
    var vm = this;
    vm.loading = false;
    vm.itemTypes = [];
    vm.remove = remove;
    vm.add = add;

    var allItemTypes = null;
    var currentItemType = null;

    function init() {
        vm.loading = true;

        switch (currentItemType) {
            case "content":
                contentTypeResource.getAll().then(getAllItemTypesCallback);
                break;
            case "media":
                mediaTypeResource.getAll().then(getAllItemTypesCallback);
                break;
            case "member":
                memberTypeResource.getTypes().then(getAllItemTypesCallback);
                break;
        }
    }

    function getAllItemTypesCallback(all) {
        allItemTypes = all;
        vm.loading = false;
        // the model value is a comma separated list of content type aliases
        var currentItemTypes = _.map(($scope.model.value || "").split(","), function (s) { return s.trim(); });
        vm.itemTypes = _.filter(allItemTypes, function (itemType) {
            return currentItemTypes.indexOf(itemType.alias) >= 0;
        });
    }

    function add() {
        if (!currentItemType) {
            return;
        }

        var editor = {
            multiPicker: true,
            filterCssClass: "not-allowed not-published",
            filter: function (item) {
                // filter out folders (containers), element types (for content) and already selected items
                return item.nodeType === "container" || item.metaData.isElement || !!_.findWhere(vm.itemTypes, { udi: item.udi });
            },
            submit: function (model) {
                var newItemTypes = _.map(model.selection,
                    function(selected) {
                        return _.findWhere(allItemTypes, { udi: selected.udi });
                    });
                vm.itemTypes = _.uniq(_.union(vm.itemTypes, newItemTypes));
                updateModel();
                editorService.close();
            },
            close: function() {
                editorService.close();
            }
        };

        switch (currentItemType) {
            case "content":
                editorService.contentTypePicker(editor);
                break;
            case "media":
                editorService.mediaTypePicker(editor);
                break;
            case "member":
                editorService.memberTypePicker(editor);
                break;
        }
    }

    function remove(itemType) {
        vm.itemTypes = _.without(vm.itemTypes, itemType);
        updateModel();
    }

    function updateModel() {
        // the model value is a comma separated list of content type aliases
        $scope.model.value = _.pluck(vm.itemTypes, "alias").join();
        angularHelper.getCurrentForm($scope).$setDirty();
    }
    var evts = [];
    evts.push(eventsService.on("treeSourceChanged", function (e, args) {
        // reset the model value if we changed node type (but not on the initial load)
        if (!!currentItemType && currentItemType !== args.value) {
            vm.itemTypes = [];
            updateModel();
        }
        currentItemType = args.value;
        init();
    }));

    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });

    if ($scope.model.config.itemType) {
        currentItemType = $scope.model.config.itemType;
        init();
    }
}

angular.module('umbraco').controller("Umbraco.PrevalueEditors.TreeSourceTypePickerController", TreeSourceTypePickerController);
