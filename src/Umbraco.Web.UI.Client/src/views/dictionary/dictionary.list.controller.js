/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.ListController
 * @function
 * 
 * @description
 * The controller for listting dictionary items
 */
function DictionaryListController($scope, $location, dictionaryResource, localizationService, appState) {
    var vm = this;
    vm.title = "Dictionary overview";
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
        var currentSection = appState.getSectionState("currentSection");
        $location.path("/" + currentSection + "/dictionary/edit/" + id);
    }

    vm.clickItem = clickItem;

    function onInit() {
        localizationService.localize("dictionaryItem_overviewTitle").then(function (value) {
            vm.title = value;
        });

        loadList();
    }

    onInit();
}


angular.module("umbraco").controller("Umbraco.Editors.Dictionary.ListController", DictionaryListController);
