function includePropsPreValsController($rootScope, $scope, localizationService, contentTypeResource) {

    if (!$scope.model.value) {
        $scope.model.value = [];
    }

    $scope.propertyAliases = [];
    $scope.selectedField = null;
    $scope.systemFields = [
        { value: "updateDate", name: "Last edited" },
        { value: "updater", name: "Updated by" },
        { value: "createDate", name: "Created" },
        { value: "owner", name: "Created by" }
    ];

    $scope.getLocalizedKey = function(alias) {
        switch (alias) {
            case "updateDate":
                return "content_updateDate";
            case "updater":
                return "content_updatedBy";
            case "createDate":
                return "content_createDate";
            case "owner":
                return "content_createBy";
        }
        return alias;
    }

    $scope.removeField = function(e) {
        $scope.model.value = _.reject($scope.model.value, function (x) {
            return x.alias === e.alias;
        });
    }

    //now we'll localize these strings, for some reason the directive doesn't work inside of the select group with an ng-model declared
    _.each($scope.systemFields, function (e, i) {
        var key = $scope.getLocalizedKey(e.value);
        localizationService.localize(key).then(function (v) {
            e.name = v;
        });
    });

    // Return a helper with preserved width of cells
    var fixHelper = function (e, ui) {
        var h = ui.clone();

        h.children().each(function () {
            $(this).width($(this).width());            
        });
        h.css("background-color", "lightgray");

        return h;
    };

    $scope.sortableOptions = {
        helper: fixHelper,
        handle: ".handle",
        opacity: 0.5,
        axis: 'y',
        containment: 'parent',
        cursor: 'move',
        items: '> tr',
        tolerance: 'pointer',
        update: function (e, ui) {
            
            // Get the new and old index for the moved element (using the text as the identifier)
            var newIndex = ui.item.index();
            var movedAlias = $('.alias-value', ui.item).text();
            var originalIndex = getAliasIndexByText(movedAlias);

            // Move the element in the model
            if (originalIndex > -1) {
                var movedElement = $scope.model.value[originalIndex];
                $scope.model.value.splice(originalIndex, 1);
                $scope.model.value.splice(newIndex, 0, movedElement);
            }
        }
    };

    contentTypeResource.getAllPropertyTypeAliases().then(function(data) {
        $scope.propertyAliases = data;
    });

    $scope.addField = function () {

        var val = $scope.selectedField;
        var isSystem = val.startsWith("_system_");
        if (isSystem) {
            val = val.trimStart("_system_");
        }

        var exists = _.find($scope.model.value, function (i) {
            return i.alias === val;
        });
        if (!exists) {
            $scope.model.value.push({
                alias: val,
                isSystem: isSystem ? 1 : 0
            });
        }
    }

    function getAliasIndexByText(value) {
        for (var i = 0; i < $scope.model.value.length; i++) {
            if ($scope.model.value[i].alias === value) {
                return i;
            }
        }

        return -1;
    }

}


angular.module("umbraco").controller("Umbraco.PrevalueEditors.ListViewController", includePropsPreValsController);