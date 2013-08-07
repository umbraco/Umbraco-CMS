/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function ContentSortController($scope, $element, contentResource, treeService, navigationService) {

    //defines the options for the jquery sortable
    $scope.sortableOptions = {
        axis: 'y',
        cursor: "move",
        placeholder: "ui-sortable-placeholder",
        update: function (ev, ui) {
            $(ui.item).effect("highlight", { color: "#049cdb" }, 500);
        }
    };

    contentResource.getChildren($scope.currentNode.id).then(function(data) {
        $scope.itemsToSort = data.items;
    });
    
    $scope.performSort = function() {

        //contentResource.sort($scope.currentNode.id).then(function () {
        //    $scope.currentNode.loading = false;

        //    //get the root node before we remove it
        //    var rootNode = treeService.getTreeRoot($scope.currentNode);

        //    //TODO: Need to sync tree, etc...
        //    treeService.removeNode($scope.currentNode);

        //    //ensure the recycle bin has child nodes now            
        //    var recycleBin = treeService.getDescendantNode(rootNode, -20);
        //    recycleBin.hasChildren = true;

        //    navigationService.hideMenu();
        //});

    };

}

angular.module("umbraco").controller("Umbraco.Editors.Content.SortController", ContentSortController);
