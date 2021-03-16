(function () {
  'use strict';

  /**
   * A component to render the content type property
   */
  
  function umbContentTypePropertyController() {

    const vm = this;

    vm.removePromptIsVisible = false;

    vm.edit = edit;
    vm.togglePrompt = togglePrompt;
    vm.hidePrompt = hidePrompt;
    vm.remove = remove;

    function edit () {
      if (vm.onEdit) {
        vm.onEdit();
      }
    }

    function togglePrompt () {
      vm.removePromptIsVisible = !vm.removePromptIsVisible;
    }

    function hidePrompt () {
      vm.removePromptIsVisible = false;
    }

    function remove () {
      if (vm.onRemove) {
        vm.onRemove({ property: vm.property });
        vm.removePromptIsVisible = false;
      }
    }

  }

  const umbContentTypePropertyComponent = {
    templateUrl: 'views/components/contenttype/umb-content-type-property.html',
    bindings: {
      property: '<',
      sortable: '<',
      onEdit: '&',
      onRemove: '&',
      valServerFieldAlias: '@',
      valServerFieldLabel: '@'
    },
    controllerAs: 'vm',
    controller: umbContentTypePropertyController
  };

  angular.module('umbraco.directives').component('umbContentTypeProperty', umbContentTypePropertyComponent);

})();
