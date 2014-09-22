/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentType.ListViewController
 * @function
 * 
 * @description
 * The controller for the customize list view dialog for content types
 */
function ContentTypeListViewController($scope, contentTypeResource, dataTypeResource) {


    function init() {
        contentTypeResource.getAssignedListViewDataType($scope.dialogOptions.contentTypeId)
            .then(function(d) {
                $scope.listViewName = d.name;
                $scope.isSystem = d.isSystem;
                $scope.dataTypeId = d.id;
            });
    }

    $scope.listViewName = "";
    $scope.isSystem = true;
    $scope.dataTypeId = 0;

    $scope.createCustom = function() {
        dataTypeResource.save({
                id: 0,
                name: "List View - " + $scope.dialogOptions.contentTypeAlias,
                selectedEditor: "Umbraco.ListView"
            }, [], true)
            .then(function(d) {
                $scope.listViewName = d.name;
                $scope.isSystem = d.isSystem;
                $scope.dataTypeId = d.id;
        });
    }

    $scope.removeCustom = function() {
        if (!$scope.isSystem && $scope.dataTypeId > 0) {
            dataTypeResource.deleteById($scope.dataTypeId)
                .then(function() {
                    init();
                });
        }
    }

    init();
}

angular.module("umbraco").controller("Umbraco.Editors.ContentType.ListViewController", ContentTypeListViewController);