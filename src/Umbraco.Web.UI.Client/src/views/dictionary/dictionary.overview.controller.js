/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.ListController
 * @function
 * 
 * @description
 * The controller for listting dictionary items
 */
function DictionaryListController($scope, $location, dictionaryResource) {
    vm = this;
    vm.title = "TODO dictoinary item";
    vm.loading = false;
}


angular.module("umbraco").controller("Umbraco.Editors.Dictionary.ListController", DictionaryListController);