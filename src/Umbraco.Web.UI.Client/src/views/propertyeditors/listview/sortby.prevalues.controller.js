function sortByPreValsController($rootScope, $scope, localizationService, editorState) {
    //Watch the prevalues
    $scope.$watch(function () {
        return _.findWhere(editorState.current.preValues, { key: "includeProperties" }).value;
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
        var propsPreValue = _.findWhere(editorState.current.preValues, { key: "includeProperties" });

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
        var systemFields = [
            { value: "SortOrder", key: "general_sort" },
            { value: "Name", key: "general_name" },
            { value: "VersionDate", key: "content_updateDate" },
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
                    switch (e.value) {
                        case "Updater":
                            e.name += " (Content only)";
                            break;
                        case "Published":
                            e.name += " (Content only)";
                            break;
                        case "Email":
                            e.name += " (Members only)";
                            break;
                        case "Username":
                            e.name += " (Members only)";
                            break;
                    }
                }
            });
        });
    }
}


angular.module("umbraco").controller("Umbraco.PrevalueEditors.SortByListViewController", sortByPreValsController);