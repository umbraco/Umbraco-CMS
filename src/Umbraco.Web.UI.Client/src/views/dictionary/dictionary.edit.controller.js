﻿/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.EditController
 * @function
 * 
 * @description
 * The controller for editing dictionary items
 */
function DictionaryEditController($scope, $routeParams, $location, dictionaryResource, navigationService, appState, editorState, contentEditingHelper, formHelper, notificationsService, localizationService) {
    
    var vm = this;

    //setup scope vars
    vm.nameDirty = false;
    vm.page = {};
    vm.page.loading = false;
    vm.page.nameLocked = false;
    vm.page.menu = {};
    vm.page.menu.currentSection = appState.getSectionState("currentSection");
    vm.page.menu.currentNode = null;
    vm.description = "";
    vm.showBackButton = true;
    
    vm.save = saveDictionary;
    vm.back = back;
  
    function loadDictionary() {

        vm.page.loading = true;

        //we are editing so get the content item from the server
        dictionaryResource.getById($routeParams.id)
            .then(function (data) {
                bindDictionary(data);
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

    function bindDictionary(data) {
        localizationService.localize("dictionaryItem_description").then(function (value) {
            vm.description = value.replace("%0%", data.name);
        });

        // create data for  umb-property displaying
        for (var i = 0; i < data.translations.length; i++) {
            data.translations[i].property = createTranslationProperty(data.translations[i]);
        }

        contentEditingHelper.handleSuccessfulSave({
            scope: $scope,
            savedContent: data
        });

        // set content
        vm.content = data;

        //share state
        editorState.set(vm.content);

        navigationService.syncTree({ tree: "dictionary", path: data.path, forceReload: true }).then(function (syncArgs) {
            vm.page.menu.currentNode = syncArgs.node;
        });
    }

    function onInit() {
        loadDictionary();
    }

    function saveDictionary() {
        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

            vm.page.saveButtonState = "busy";

            dictionaryResource.save(vm.content, vm.nameDirty)
                .then(function (data) {

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                        bindDictionary(data);

                        vm.page.saveButtonState = "success";
                    },
                    function (err) {

                        contentEditingHelper.handleSaveError({
                            redirectOnFailure: false,
                            err: err
                        });
                        
                        notificationsService.error(err.data.message);

                        vm.page.saveButtonState = "error";
                    });
        }
    }
    
    function back() {
        $location.path(vm.page.menu.currentSection + "/dictionary/list");
    }

    $scope.$watch("vm.content.name", function (newVal, oldVal) {
        //when the value changes, we need to set the name dirty
        if (newVal && (newVal !== oldVal) && typeof(oldVal) !== "undefined") {
            vm.nameDirty = true;           
        }
    });

    onInit();
}

angular.module("umbraco").controller("Umbraco.Editors.Dictionary.EditController", DictionaryEditController);
