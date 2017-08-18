/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.DeleteController
 * @function
 * 
 * @description
 * The controller for deleting dictionary items
 */
function DictionaryDeleteController($scope, dictionaryResource, treeService, navigationService) {
  vm = this;

  function cancel() {
    navigationService.hideDialog();
  }

  function performDelete() {
    // stop from firing again on double-click
    if ($scope.busy) { return false; }

    //mark it for deletion (used in the UI)
    $scope.currentNode.loading = true;
    $scope.busy = true;

    dictionaryResource.deleteById($scope.currentNode.id).then(function () {
      $scope.currentNode.loading = false;
      
      treeService.removeNode($scope.currentNode);

      //if the current edited item is the same one as we're deleting, we need to navigate elsewhere
      if (editorState.current && editorState.current.id === $scope.currentNode.id) {

        //If the deleted item lived at the root then just redirect back to the root, otherwise redirect to the item's parent
        var location = "/settings";
        if ($scope.currentNode.parentId.toString() !== "-1")
          location = "/settings/current/edit/" + $scope.currentNode.parentId;

        $location.path(location);
      }

      navigationService.hideMenu();
    });
  }

  vm.cancel = cancel;
  vm.performDelete = performDelete;
}

angular.module("umbraco").controller("Umbraco.Editors.Dictionary.DeleteController", DictionaryDeleteController);