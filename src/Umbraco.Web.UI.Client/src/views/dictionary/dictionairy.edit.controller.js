﻿/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.EditController
 * @function
 * 
 * @description
 * The controller for editing dictionary items
 */
function DictionaryEditController($scope, $routeParams, dictionaryResource, treeService, navigationService, appState, editorState, contentEditingHelper, formHelper) {
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

                // create data for  umb-property displaying
                for(var i=0; i<data.translations.length;i++) {
                    data.translations[i].property = createTranslationProperty(data.translations[i]);
                }
                
                // set content
                vm.content = data;                

                //share state
                editorState.set(vm.content);
               
                navigationService.syncTree({ tree: "dictionary", path: data.path }).then(function (syncArgs) {
                   vm.page.menu.currentNode = syncArgs.node;
                });

                vm.page.loading = false;

            });
    }

    function createTranslationProperty(translation) {
        return {
            alias: translation.isoCode,
            label: translation.displayName,
            hideLabel : false
        }
    }  

    function onInit() {
        loadDictionary();
    }

    function saveDictionary() {
        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

            vm.page.saveButtonState = "busy";

            dictionaryResource.save(vm.content, false)
                .then(function(data) {
                        
                        vm.page.saveButtonState = "success";
                    },
                    function (err) {
                        contentEditingHelper.handleSaveError({
                            redirectOnFailure: false,
                            err: err
                        });

                        vm.page.saveButtonState = "error";
                    });
        }
    }

    vm.save = saveDictionary;

    onInit();
}

angular.module("umbraco").controller("Umbraco.Editors.Dictionary.EditController", DictionaryEditController);