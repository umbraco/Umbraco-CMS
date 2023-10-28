/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentDeleteController
 * @function
 * 
 * @description
 * The controller for deleting content
 */
function MediaDeleteController($scope, mediaResource, treeService, navigationService, editorState, $location, overlayService, localizationService) {

    $scope.checkingReferences = true;
    $scope.warningText = null;
    $scope.disableDelete = false;

    $scope.performDelete = function() {

        // stop from firing again on double-click
        if ($scope.busy) { return false; }

        //mark it for deletion (used in the UI)
        $scope.currentNode.loading = true;
        $scope.busy = true;

        mediaResource.deleteById($scope.currentNode.id).then(function () {
            $scope.currentNode.loading = false;

            //get the root node before we remove it
            var rootNode = treeService.getTreeRoot($scope.currentNode);

            treeService.removeNode($scope.currentNode);

            if (rootNode) {
                //ensure the recycle bin has child nodes now
                var recycleBin = treeService.getDescendantNode(rootNode, -21);
                if (recycleBin) {
                    recycleBin.hasChildren = true;
                    //reload the recycle bin if it's already expanded so the deleted item is shown
                    if (recycleBin.expanded) {
                        treeService.loadNodeChildren({ node: recycleBin, section: "media" });
                    }
                }
            }
            
            //if the current edited item is the same one as we're deleting, we need to navigate elsewhere
            if (editorState.current && editorState.current.id == $scope.currentNode.id) {

            	//If the deleted item lived at the root then just redirect back to the root, otherwise redirect to the item's parent
                var location = "/media";
                if ($scope.currentNode.parentId.toString() === "-21")
                    location = "/media/media/recyclebin";
            	else if ($scope.currentNode.parentId.toString() !== "-1")
            		location = "/media/media/edit/" + $scope.currentNode.parentId;

                $location.path(location);
            }

            $scope.success = true;
        }, function (err) {

            $scope.currentNode.loading = false;
            $scope.busy = false;

            //check if response is ysod
            if (err.status && err.status >= 500) {
                // TODO: All YSOD handling should be done with an interceptor
                overlayService.ysod(err);
            }

        });
    };

    $scope.checkingReferencesComplete = () => {
        $scope.checkingReferences = false;
    };

    $scope.onReferencesWarning = () => {
        // check if the deletion of items that have references has been disabled
        if (Umbraco.Sys.ServerVariables.umbracoSettings.disableDeleteWhenReferenced) {
            // this will only be set to true if we have a warning, indicating that this item or its descendants have reference
            $scope.disableDelete = true;

            localizationService.localize("references_deleteDisabledWarning").then((value) => {
                $scope.warningText = value;
            });
        }
        else {
            localizationService.localize("references_deleteWarning").then((value) => {
                $scope.warningText = value;
            });
        }
    };

    $scope.close = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.Media.DeleteController", MediaDeleteController);
