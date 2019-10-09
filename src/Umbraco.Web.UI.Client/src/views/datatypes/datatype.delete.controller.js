/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function DataTypeDeleteController($scope, dataTypeResource, treeService, navigationService, localizationService) {

    var vm = this;

    vm.hasRelations = false;
    vm.relations = [];

    vm.performDelete = function() {

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

    vm.performContainerDelete = function () {

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

    vm.cancel = function() {
        navigationService.hideDialog();
    };

    vm.labels = {};
    localizationService
        .localize("editdatatype_acceptDeleteConsequence", [$scope.currentNode.name])
        .then(function (data) {
            vm.labels.deleteConfirm = data;
        });

    var init = function() {

        console.log($scope.currentNode);

        if($scope.currentNode.nodeType === "dataTypes") {

            vm.loading = true;

            dataTypeResource.getReferences($scope.currentNode.id)
                .then(function (data) {
                    vm.loading = false;
                    vm.relations = data;

                    console.log(data);

                    vm.hasRelations = data.documentTypes.length > 0 || data.mediaTypes.length > 0 || data.memberTypes.length > 0;
                });

        }

    }

    init();
}

angular.module("umbraco").controller("Umbraco.Editors.DataType.DeleteController", DataTypeDeleteController);
