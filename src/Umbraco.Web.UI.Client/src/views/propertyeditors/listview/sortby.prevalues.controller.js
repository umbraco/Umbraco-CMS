function sortByPreValsController($rootScope, $scope, localizationService, editorState) {
    var propsPreValue = _.findWhere(editorState.current.preValues, { key: "includeProperties" });

    if(propsPreValue != undefined){
        $scope.sortByFields = propsPreValue.value;
    }
    else {
        $scope.sortByFields = [];
    }

    var existingValue = _.find($scope.sortByFields, function (e) {
        return e.alias.toLowerCase() === $scope.model.value;
    });

    if (existingValue === undefined) {
        //Existing value not found

        //Set to first value
        if($scope.sortByFields.length > 0){
            $scope.model.value = $scope.sortByFields[0].alias;
        }
        
    }
    else {
        //Set the existing value
        //The old implementation pre Umbraco 7.5 used PascalCase aliases, this uses camelCase, so this ensures that any previous value is set
        $scope.model.value = existingValue.alias;
    }

    //$scope.sortByFields = [
    //    { value: "SortOrder", key: "general_sort" },
    //    { value: "Name", key: "general_name" },
    //    { value: "VersionDate", key: "content_updateDate" },
    //    { value: "Updater", key: "content_updatedBy" },
    //    { value: "CreateDate", key: "content_createDate" },
    //    { value: "Owner", key: "content_createBy" },
    //    { value: "ContentTypeAlias", key: "content_documentType" },
    //    { value: "Published", key: "content_isPublished" },
    //    { value: "Email", key: "general_email" },
    //    { value: "Username", key: "general_username" }
    //];
    
    ////now we'll localize these strings, for some reason the directive doesn't work inside of the select group with an ng-model declared
    //_.each($scope.sortByFields, function (e, i) {
    //    localizationService.localize(e.key).then(function (v) {
    //        e.name = v;

    //        switch (e.value) {
    //            case "Updater":
    //                e.name += " (Content only)";
    //                break;
    //            case "Published":
    //                e.name += " (Content only)";
    //                break;
    //            case "Email":
    //                e.name += " (Members only)";
    //                break;
    //            case "Username":
    //                e.name += " (Members only)";
    //                break;
    //        }
    //    });
    //});

}


angular.module("umbraco").controller("Umbraco.PrevalueEditors.SortByListViewController", sortByPreValsController);