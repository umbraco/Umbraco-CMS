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

  vm.cancel = cancel;
}

angular.module("umbraco").controller("Umbraco.Editors.Dictionary.DeleteController", DictionaryDeleteController);