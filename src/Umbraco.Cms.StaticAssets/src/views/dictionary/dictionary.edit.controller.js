/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.EditController
 * @function
 * 
 * @description
 * The controller for editing dictionary items
 */
function DictionaryEditController($scope, $routeParams, $location, dictionaryResource, navigationService, appState, editorState, contentEditingHelper, formHelper, notificationsService, localizationService, userService) {

  var vm = this;

  //setup scope vars
  vm.nameDirty = false;
  vm.header = {};
  vm.header.editorfor = "template_insertDictionaryItem";
  vm.header.setPageTitle = true;

  vm.page = {};
  vm.page.navigation = {};
  vm.page.loading = false;
  vm.page.nameLocked = false;
  vm.page.menu = {};
  vm.page.menu.currentSection = appState.getSectionState("currentSection");
  vm.page.menu.currentNode = null;
  vm.description = "";
  vm.showBackButton = true;
  vm.maxlength = 1000;
  vm.currentUser = null;

  vm.save = saveDictionary;
  vm.back = back;
  vm.change = change;

  function loadDictionary() {

    vm.page.loading = true;

    //we are editing so get the content item from the server
    dictionaryResource.getById($routeParams.id)
      .then(function (data) {
        bindDictionary(data);
        vm.page.navigation = data.apps;
        data.apps[0].active = true;
        vm.page.loading = false;
      });
  }

  function createTranslationProperty(translation) {
    return {
      alias: translation.isoCode,
      label: translation.displayName,
      hideLabel: false
    }
  }

  function bindDictionary(data) {

    data.translations.forEach(translation => {
      const allowUpdate = vm.currentUser.allowedLanguageIds && vm.currentUser.allowedLanguageIds.length > 0 && vm.currentUser.allowedLanguageIds.includes(translation.languageId);
      translation.allowUpdate = allowUpdate;
    });

    localizationService.localize("dictionaryItem_description").then(function (value) {
      vm.description = value.replace("%0%", data.name);
    });

    // create data for  umb-property displaying
    for (var i = 0; i < data.translations.length; i++) {
      data.translations[i].property = createTranslationProperty(data.translations[i]);
      change(data.translations[i]);
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
    vm.page.loading = true;

    userService.getCurrentUser().then(user => {
      vm.currentUser = user;
      vm.page.loading = false;

      loadDictionary();
    });
  }

  function saveDictionary() {
    if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

      vm.page.saveButtonState = "busy";

      dictionaryResource.save(vm.content, vm.nameDirty)
        .then(function (data) {

          formHelper.resetForm({ scope: $scope });

          bindDictionary(data);

          vm.page.saveButtonState = "success";
        },
          function (err) {

            formHelper.resetForm({ scope: $scope, hasErrors: true });

            contentEditingHelper.handleSaveError({
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

  function change(translation) {
    if (translation.translation) {
      var charsCount = translation.translation.length;
      translation.nearMaxLimit = charsCount > Math.max(vm.maxlength * .8, vm.maxlength - 50);
    }
  }

  $scope.$watch("vm.content.name", function (newVal, oldVal) {
    //when the value changes, we need to set the name dirty
    if (newVal && (newVal !== oldVal) && typeof (oldVal) !== "undefined") {
      vm.nameDirty = true;
    }
  });

  onInit();
}

angular.module("umbraco").controller("Umbraco.Editors.Dictionary.EditController", DictionaryEditController);
