function MacroListController($scope, entityResource) {

    $scope.items = [];
    
    entityResource.getAll("Macro").then(function(items) {
        _.each(items, function(i) {
            $scope.items.push({ name: i.name, alias: i.alias });
        });
        
    });


}

angular.module("umbraco").controller("Umbraco.PrevalueEditors.MacroList", MacroListController);
