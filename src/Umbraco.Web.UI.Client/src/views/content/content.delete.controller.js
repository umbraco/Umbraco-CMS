/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function ContentDeleteController($scope, contentResource, treeService, navigationService, editorState, $location, dialogService, notificationsService) {

    $scope.performDelete = function() {

        // stop from firing again on double-click
        if ($scope.busy) { return false; }

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        $scope.busy = true;

        contentResource.deleteById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            //get the root node before we remove it
            var rootNode = treeService.getTreeRoot($scope.currentNode);

            treeService.removeNode($scope.currentNode);

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
                if ($scope.currentNode.parentId.toString() !== "-1")
                    location = "/content/content/edit/" + $scope.currentNode.parentId;

                $location.path(location);
            }

            navigationService.hideMenu();
        }, function(err) {

            $scope.currentNode.loading = false;
            $scope.busy = false;

            //check if response is ysod
            if (err.status && err.status >= 500) {
                dialogService.ysodDialog(err);
            }
            
            if (err.data && angular.isArray(err.data.notifications)) {
                for (var i = 0; i < err.data.notifications.length; i++) {
                    notificationsService.showNotification(err.data.notifications[i]);
                }
            }
        });

    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.Content.DeleteController", ContentDeleteController);
