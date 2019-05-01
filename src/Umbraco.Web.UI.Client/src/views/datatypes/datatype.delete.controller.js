/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function DataTypeDeleteController($scope, dataTypeResource, treeService, navigationService, localizationService) {

    $scope.performDelete = function() {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        dataTypeResource.deleteById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            //get the root node before we remove it
            var rootNode = treeService.getTreeRoot($scope.currentNode);
            
            // TODO: Need to sync tree, etc...
            treeService.removeNode($scope.currentNode);
            navigationService.hideMenu();
        });
    };

    $scope.performContainerDelete = function () {

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        dataTypeResource.deleteContainerById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            //get the root node before we remove it
            var rootNode = treeService.getTreeRoot($scope.currentNode);

            // TODO: Need to sync tree, etc...
            treeService.removeNode($scope.currentNode);
            navigationService.hideMenu();
        });

    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };

    $scope.labels = {};
    localizationService
        .format(["editdatatype_yesDelete", "editdatatype_andAllRelated"], "%0% " + $scope.currentNode.name + " %1%")
        .then(function (data) {
            $scope.labels.deleteConfirm = data;
        });
}

angular.module("umbraco").controller("Umbraco.Editors.DataType.DeleteController", DataTypeDeleteController);
