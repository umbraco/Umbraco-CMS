/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.DeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function ContentDeleteController($scope, $timeout, contentResource, treeService, navigationService, editorState, $location, overlayService, languageResource) {

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
                //ensure the recycle bin has child nodes now            
                var recycleBin = treeService.getDescendantNode(rootNode, -20);
                if (recycleBin) {
                    recycleBin.hasChildren = true;
                    //reload the recycle bin if it's already expanded so the deleted item is shown
                    if (recycleBin.expanded) {
                        treeService.loadNodeChildren({ node: recycleBin, section: "content" });
                    }
                }
            }

            //if the current edited item is the same one as we're deleting, we need to navigate elsewhere
            if (editorState.current && editorState.current.id == $scope.currentNode.id) {

                //If the deleted item lived at the root then just redirect back to the root, otherwise redirect to the item's parent
                var location = "/content";
                if ($scope.currentNode.parentId.toString() === "-20")
                    location = "/content/content/recyclebin";
                else if ($scope.currentNode.parentId.toString() !== "-1")
                    location = "/content/content/edit/" + $scope.currentNode.parentId;

                $location.path(location);
            }

            $scope.success = true;
        }, function(err) {

            toggleDeleting(false);

            //check if response is ysod
            if (err.status && err.status >= 500) {
                // TODO: All YSOD handling should be done with an interceptor
                overlayService.ysod(err);
            }
        });

    };

    $scope.cancel = function() {
        toggleDeleting(false);
        $scope.close();
    };

    $scope.close = function () {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.Content.DeleteController", ContentDeleteController);
