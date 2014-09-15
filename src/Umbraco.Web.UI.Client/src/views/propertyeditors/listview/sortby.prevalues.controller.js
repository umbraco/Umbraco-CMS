function sortByPreValsController($rootScope, $scope, localizationService) {

    $scope.sortByFields = [
        { value: "SortOrder", key: "general_sort" },
        { value: "Name", key: "general_name" },
        { value: "UpdateDate", key: "content_updateDate" },
        { value: "Updater", key: "content_updatedBy" },
        { value: "CreateDate", key: "content_createDate" },
        { value: "Owner", key: "content_createBy" },
        { value: "ContentTypeAlias", key: "content_documentType" },
        { value: "Published", key: "content_isPublished" }
    ];
    
    //now we'll localize these strings, for some reason the directive doesn't work inside of the select group with an ng-model declared
    _.each($scope.sortByFields, function (e, i) {
        localizationService.localize(e.key).then(function (v) {
            e.name = v;
        });
    });

}


angular.module("umbraco").controller("Umbraco.PrevalueEditors.SortByListViewController", sortByPreValsController);