/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.CreateController
 * @function
 * 
 * @description
 * The controller for creating dictionary items
 */
function DictionaryCreateController($scope, dictionaryResource, treeService, navigationService) {
  vm = this;

  vm.itemKey = '';

  function createItem() {
    console.log(vm.itemKey);
    console.log($scope.dialogOptions.currentNode.id);

    // do actual saving
    navigationService.hideMenu();
  }

  vm.createItem = createItem;
}

angular.module("umbraco").controller("Umbraco.Editors.Dictionary.CreateController", DictionaryCreateController);