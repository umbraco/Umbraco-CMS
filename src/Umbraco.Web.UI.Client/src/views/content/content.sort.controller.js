/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function ContentSortController($scope, contentResource, angularHelper) {

    $scope.complete = false;

    //defines the options for the jquery sortable
    $scope.sortableOptions = {
        axis: 'y',
        cursor: "move",
        placeholder: "ui-sortable-placeholder",
        update: function (ev, ui) {
            //highlight the item when the position is changed
            $(ui.item).effect("highlight", { color: "#049cdb" }, 500);
        },
        stop: function (ev, ui) {
            //the ui-sortable directive already ensures that our list is re-sorted, so now we just
            // need to update the sortOrder to the index of each item
            angularHelper.safeApply($scope, function () {                
                angular.forEach($scope.itemsToSort, function (val, index) {
                    val.sortOrder = index + 1;
                });

            });
        }
    };

    contentResource.getChildren($scope.currentNode.id).then(function(data) {
        $scope.itemsToSort = data.items;
    });
    
    $scope.performSort = function() {

        var sortedIds = [];
        for (var i = 0; i < $scope.itemsToSort.length; i++) {
            sortedIds.push($scope.itemsToSort[i].id);
        }
        contentResource.sort({ parentId: $scope.currentNode.id, sortedIds: sortedIds })
            .then(function() {
                $scope.complete = true;
            });

    };

}

angular.module("umbraco").controller("Umbraco.Editors.Content.SortController", ContentSortController);
