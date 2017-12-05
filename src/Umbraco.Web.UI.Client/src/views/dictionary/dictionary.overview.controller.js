/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.DictionaryOverviewController
 * @function
 * 
 * @description
 * The controller for listting dictionary items
 */
function DictionaryOverviewController($scope, $location, dictionaryResource) {
    vm = this;
    vm.title = "TODO dictoinary item"
    vm.loading = false;
}


angular.module("umbraco").controller("Umbraco.Editors.Dictionary.OverviewController", DictionaryOverviewController);