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

    vm.loading = true;
}


angular.module("umbraco").controller("Umbraco.Editors.Dictionary.OverviewController", DictionaryOverviewController);