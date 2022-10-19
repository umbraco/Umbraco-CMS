/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.ListController
 * @function
 * 
 * @description
 * The controller for listting dictionary items
 */
function DictionaryListController($scope, $location, dictionaryResource, localizationService, appState, navigationService) {

    const vm = this;
    
    vm.title = "Dictionary overview";
    vm.loading = false;
    vm.items = [];
    vm.filter = {
      searchTerm: ""
    };

    function loadList() {

        vm.loading = true;
        
        dictionaryResource.getList()
            .then(data => {
                let items = data || [];
                
                items.forEach(item => {
                  item.style = { "paddingLeft": item.level * 10 };
                });
                vm.items = items;
                vm.loading = false;
            });
    }

    function clickItem(id) {
        var currentSection = appState.getSectionState("currentSection");
        $location.path("/" + currentSection + "/dictionary/edit/" + id);
    }

    function createNewItem() {
        var rootNode = appState.getTreeState("currentRootNode").root;
        //We need to load the menu first before we can access the menu actions.
        navigationService.showMenu({ node: rootNode }).then(function () {
            const action = appState.getMenuState("menuActions").find(item => item.alias === "create");
            navigationService.executeMenuAction(action, rootNode, appState.getSectionState("currentSection"));
        });
      }

    vm.clickItem = clickItem;
    vm.createNewItem = createNewItem;

    function onInit() {
        localizationService.localize("dictionaryItem_overviewTitle").then(value => {
            vm.title = value;
        });

        loadList();
    }

    onInit();
}


angular.module("umbraco").controller("Umbraco.Editors.Dictionary.ListController", DictionaryListController);
