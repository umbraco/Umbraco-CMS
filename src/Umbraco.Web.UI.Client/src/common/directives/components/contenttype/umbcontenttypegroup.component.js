(function () {
  'use strict';

  /**
   * A component to render the content type group
   */
  
  function umbContentTypeGroupController() {

    const vm = this;

    vm.removePromptIsVisible = false;

    vm.updateName = updateName;
    vm.removeGroup = removeGroup;
    vm.togglePrompt = togglePrompt;
    vm.hidePrompt = hidePrompt;
    vm.whenNameFocus = whenNameFocus;

    function togglePrompt () {
      vm.removePromptIsVisible = !vm.removePromptIsVisible;
    }

    function hidePrompt () {
      vm.removePromptIsVisible = false;
    }

    function updateName (group) {
      if (vm.onUpdateName) {
        vm.onUpdateName({ group });
      }
    }

    function removeGroup () {
      if (vm.onRemove) {
        vm.onRemove({ group: vm.group });
        vm.removePromptIsVisible = false;
      }
    }

    function whenNameFocus () {
      if (vm.onNameFocus) {
        vm.onNameFocus();
      }
    }
  }

  const umbContentTypeGroupComponent = {
    templateUrl: 'views/components/contenttype/umb-content-type-group.html',
    controllerAs: 'vm',
    transclude: true,
    bindings: {
      group: '<',
      allowName: '<',
      onUpdateName: '&',
      allowRemove: '<',
      onRemove: '&',
      sorting: '<',
      nameValServerField: '@',
      onNameFocus: '&'
    },
    controller: umbContentTypeGroupController
  };

  angular.module('umbraco.directives').component('umbContentTypeGroup', umbContentTypeGroupComponent);
})();
