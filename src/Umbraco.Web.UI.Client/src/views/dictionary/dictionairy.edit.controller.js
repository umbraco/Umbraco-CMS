/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.EditController
 * @function
 * 
 * @description
 * The controller for editing dictionary items
 */
function DictionaryEditController($scope, dictionaryResource, treeService, navigationService, appState, $routeParams, editorState) {
  vm = this;

    //setup scope vars
    vm.page = {};
    vm.page.loading = false;
    vm.page.nameLocked = false;
    vm.page.menu = {};
    vm.page.menu.currentSection = appState.getSectionState("currentSection");
    vm.page.menu.currentNode = null;
  
    function loadDictionary() {

        vm.page.loading = true;

        //we are editing so get the content item from the server
        dictionaryResource.getById($routeParams.id)
            .then(function (data) {
                
                //
                vm.content = data;                

                //share state
                editorState.set(vm.content);
               
                navigationService.syncTree({ tree: "dictionary", path: data.path }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });

                vm.page.loading = false;

            });
    }

    function tranformTranslationToProperty(translation) {
        return {
            alias: translation.isoCode,
            label: translation.displayName,
            hideLabel : false
        }
    }

    vm.tranformTranslationToProperty = tranformTranslationToProperty;

    function onInit() {
        loadDictionary();
    }

    onInit();
}

angular.module("umbraco").controller("Umbraco.Editors.Dictionary.EditController", DictionaryEditController);