/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function ContentDeleteController($scope, $timeout, contentResource, treeService, navigationService, editorState, $location, overlayService) {

    /**
     * Used to toggle UI elements during delete operations
     * @param {any} isDeleting
     */
    function toggleDeleting(isDeleting) {
        $scope.currentNode.loading = isDeleting;
        $scope.busy = isDeleting;
    }
    
    $scope.performDelete = function() {

        // stop from firing again on double-click
        if ($scope.busy) { return false; }

        toggleDeleting(true);

        contentResource.deleteById($scope.currentNode.id).then(function () {
            
            //get the root node before we remove it
            var rootNode = treeService.getTreeRoot($scope.currentNode);

            treeService.removeNode($scope.currentNode);

            toggleDeleting(false);

            if (rootNode) {
                $timeout(function () {
                    //ensure the recycle bin has child nodes now            
                    var recycleBin = treeService.getDescendantNode(rootNode, -20);
                    if (recycleBin) {
                        //TODO: This seems to return a rejection and we end up with "Possibly unhanded rejection"
                        treeService.syncTree({ node: recycleBin, path: treeService.getPath(recycleBin), forceReload: true });
                    }
                }, 500);
            }
            
            //if the current edited item is the same one as we're deleting, we need to navigate elsewhere
            if (editorState.current && editorState.current.id == $scope.currentNode.id) {

                //If the deleted item lived at the root then just redirect back to the root, otherwise redirect to the item's parent
                var location = "/content";
                if ($scope.currentNode.parentId.toString() !== "-1")
                    location = "/content/content/edit/" + $scope.currentNode.parentId;

                $location.path(location);
            }

            navigationService.hideMenu();
        }, function(err) {

            toggleDeleting(false);

            //check if response is ysod
            if (err.status && err.status >= 500) {
                overlayService.ysod(err);
            }
        });

    };

    $scope.cancel = function() {
        toggleDeleting(false);
        navigationService.hideDialog();        
    };
}

angular.module("umbraco").controller("Umbraco.Editors.Content.DeleteController", ContentDeleteController);
