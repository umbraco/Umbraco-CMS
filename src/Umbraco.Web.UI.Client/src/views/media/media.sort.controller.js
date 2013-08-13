/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function MediaSortController($scope, mediaResource, treeService) {

    $scope.sortableModel = {
        itemsToSort: [],
        name: $scope.nav.ui.currentNode.name
    };

    mediaResource.getChildren($scope.currentNode.id).then(function (data) {
        $scope.sortableModel.itemsToSort = [];
        for (var i = 0; i < data.length; i++) {
            $scope.sortableModel.itemsToSort.push({
                id: data[i].id,
                column1: data[i].name,
                column2: data[i].updateDate,
                column3: data[i].sortOrder
            });
        }
    });

    $scope.$on("umbItemSorter.ok", function (event) {
        $scope.nav.hideDialog();
    });
    $scope.$on("umbItemSorter.cancel", function (event) {
        $scope.nav.hideDialog();
    });

    $scope.$on("umbItemSorter.sorting", function (event, args) {

        var sortedIds = [];
        for (var i = 0; i < args.sortedItems.length; i++) {
            sortedIds.push(args.sortedItems[i].id);
        }
        mediaResource.sort({ parentId: $scope.currentNode.id, sortedIds: sortedIds })
            .then(function () {
                $scope.sortableModel.complete = true;
                treeService.loadNodeChildren({ node: $scope.nav.ui.currentNode, section: $scope.nav.ui.currentSection });
            });

    });

}

angular.module("umbraco").controller("Umbraco.Editors.Media.SortController", MediaSortController);
