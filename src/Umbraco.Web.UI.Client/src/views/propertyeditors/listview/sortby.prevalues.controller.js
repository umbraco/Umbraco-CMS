function sortByPreValsController($rootScope, $scope, localizationService, editorState, listViewPrevalueHelper) {
    //Get the prevalue from the correct place
    function getPrevalues() {
        if (editorState.current.preValues) {
            return editorState.current.preValues;
        }
        else {
            return listViewPrevalueHelper.getPrevalues();
        }
    }

    //Watch the prevalues
    $scope.$watch(function () {
            return _.findWhere(getPrevalues(), { key: "includeProperties" }).value;
    }, function () {
        populateFields();
    }, true); //Use deep watching, otherwise we won't pick up header changes

    function populateFields() {
        // Helper to find a particular value from the list of sort by options
        function findFromSortByFields(value) {
            return _.find($scope.sortByFields, function (e) {
                return e.value.toLowerCase() === value.toLowerCase();
            });
        }

        // Get list of properties assigned as columns of the list view
        var propsPreValue = _.findWhere(getPrevalues(), { key: "includeProperties" });

        // Populate list of options for the default sort (all the columns plus then node name)
        $scope.sortByFields = [];
        $scope.sortByFields.push({ value: "name", name: "Name", isSystem: 1 });
        if (propsPreValue != undefined) {
            for (var i = 0; i < propsPreValue.value.length; i++) {
                var value = propsPreValue.value[i];
                $scope.sortByFields.push({
                    value: value.alias,
                    name: value.header,
                    isSystem: value.isSystem
                });
            }
        }

        // Localize the system fields, for some reason the directive doesn't work inside of the select group with an ng-model declared
	// beware: ensure that GetDatabaseFieldNameForOrderBy knows about those fields!
        var systemFields = [
            { value: "SortOrder", key: "general_sort" },
            { value: "Name", key: "general_name" },
            { value: "UpdateDate", key: "content_updateDate" },
            { value: "Updater", key: "content_updatedBy" },
            { value: "CreateDate", key: "content_createDate" },
            { value: "Owner", key: "content_createBy" },
            { value: "ContentTypeAlias", key: "content_documentType" },
            { value: "Published", key: "content_isPublished" },
            { value: "Email", key: "general_email" },
            { value: "Username", key: "general_username" }
        ];
        _.each(systemFields, function (e) {
            localizationService.localize(e.key).then(function (v) {
                var sortByListValue = findFromSortByFields(e.value);
                if (sortByListValue) {
                    sortByListValue.name = v;
                }
            });
        });

        _.each($scope.sortByFields, function (sortByField) {
            if (!sortByField.name) {
                sortByField.name = "(" + sortByField.value + ")";
            }
        });

        // Check existing model value is available in list and ensure a value is set
        var existingValue = findFromSortByFields($scope.model.value);
        if (existingValue) {
            // Set the existing value
            // The old implementation pre Umbraco 7.5 used PascalCase aliases, this uses camelCase, so this ensures that any previous value is set
            $scope.model.value = existingValue.value;
        }
        else {
            // Existing value not found, set to first value
            $scope.model.value = $scope.sortByFields[0].value;
        }
    }
}


angular.module("umbraco").controller("Umbraco.PrevalueEditors.SortByListViewController", sortByPreValsController);
