/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.CreateController
 * @function
 * 
 * @description
 * The controller for creating dictionary items
 */
function DictionaryCreateController($scope, dictionaryResource, treeService, navigationService, notificationsService) {
  vm = this;

  vm.itemKey = '';

  function createItem() {    

    dictionaryResource.create($scope.dialogOptions.currentNode.id, vm.itemKey).then(function(data) {
      console.log(data);
      navigationService.hideMenu();
    },function(err) {
      console.log(err);
    });
  }

  vm.createItem = createItem;
}

angular.module("umbraco").controller("Umbraco.Editors.Dictionary.CreateController", DictionaryCreateController);