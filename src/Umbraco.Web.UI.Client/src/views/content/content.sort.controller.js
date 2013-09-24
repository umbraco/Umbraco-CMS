/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function ContentSortController($scope, contentResource, treeService) {
    contentResource.getChildren($scope.currentNode.id).then(function (data) {
        $scope.pagesToSort = [];
        for (var i = 0; i < data.items.length; i++) {
            $scope.pagesToSort.push({
                id: data.items[i].id,
                name: data.items[i].name,
                updateDate: data.items[i].updateDate,
                sortOrder: data.items[i].sortOrder
            });
        }
    });

    $scope.sortOptions ={
        group: "pages",
        containerSelector: 'table',
        itemPath: '> tbody',
        itemSelector: 'tr',
        placeholder: '<tr class="placeholder"/>',
        clone: "<tr />",
        mode: "table",
        onSortHandler: function(item, args){


            args.scope.changeIndex(args.oldIndex, args.newIndex);
        }
    };

/*
    $scope.$on("umbItemSorter.sorting", function (event, args) {

        var sortedIds = [];
        for (var i = 0; i < args.sortedItems.length; i++) {
            sortedIds.push(args.sortedItems[i].id);
        }
        contentResource.sort({ parentId: $scope.currentNode.id, sortedIds: sortedIds })
            .then(function () {
                $scope.sortableModel.complete = true;
                treeService.loadNodeChildren({ node: $scope.nav.ui.currentNode, section: $scope.nav.ui.currentSection });
            });
    });*/

}

angular.module("umbraco").controller("Umbraco.Editors.Content.SortController", ContentSortController);
