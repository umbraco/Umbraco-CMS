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
}


angular.module("umbraco").controller("Umbraco.PrevalueEditors.SortByListViewController", sortByPreValsController);