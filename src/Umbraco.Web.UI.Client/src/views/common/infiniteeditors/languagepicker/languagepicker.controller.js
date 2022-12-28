(function () {
  "use strict";

  function LanguagePickerController($scope, languageResource, localizationService) {

    var vm = this;

    vm.languages = [];
    vm.loading = false;

    vm.selectLanguage = selectLanguage;
    vm.submit = submit;
    vm.close = close;

    function onInit() {

      vm.loading = true;

      // set default title
      if (!$scope.model.title) {
        localizationService.localize("defaultdialogs_selectLanguages").then(function (value) {
          $scope.model.title = value;
        });
      }

      // make sure we can push to something
      if (!$scope.model.selection) {
        $scope.model.selection = [];
      }

      // get languages
      languageResource.getAll().then(function (languages) {
        vm.languages = languages;

        if ($scope.model.selection && $scope.model.selection.length > 0) {
          preSelect($scope.model.selection);
        }

        vm.loading = false;
      });

    }

    function preSelect(selection) {
      selection.forEach(function (selected) {
        vm.languages.forEach(function (language) {
          if (selected.id === language.id) {
            language.selected = true;
          }
        });
      });
    }

    function selectLanguage(language) {

      if (!language.selected) {

        language.selected = true;
        $scope.model.selection.push(language);

      } else {

        $scope.model.selection.forEach(function (selectedLanguage, index) {
          if (selectedLanguage.id === language.id) {
            language.selected = false;
            $scope.model.selection.splice(index, 1);
          }
        });

      }

    }

    function submit(model) {
      if ($scope.model.submit) {
        $scope.model.submit(model);
      }
    }

    function close() {
      if ($scope.model.close) {
        $scope.model.close();
      }
    }

    onInit();

  }

  angular.module("umbraco").controller("Umbraco.Editors.LanguagePickerController", LanguagePickerController);

})();
