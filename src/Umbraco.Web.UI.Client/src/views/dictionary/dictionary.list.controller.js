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
    vm.items = [];   

    function loadList() {

        vm.loading = true;
        
        dictionaryResource.getList()
            .then(function (data) {

                vm.items = data;

                vm.loading = false;
            });
    }

    function clickItem(id) {
        $location.path("/settings/dictionary/edit/" + id);
    }

    vm.clickItem = clickItem;

    function onInit() {
        loadList();
    }

    onInit();
}


angular.module("umbraco").controller("Umbraco.Editors.Dictionary.ListController", DictionaryListController);