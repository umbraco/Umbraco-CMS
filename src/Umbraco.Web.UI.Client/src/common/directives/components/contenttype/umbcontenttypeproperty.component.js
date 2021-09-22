(function () {
  'use strict';

  /**
   * A component to render the content type property
   */
  
  function umbContentTypePropertyController() {

    const vm = this;

    vm.edit = edit;
    vm.remove = remove;
    vm.changeSortOrderValue = changeSortOrderValue;

    function edit () {
      if (vm.onEdit) {
        vm.onEdit();
      }
    }

    function remove () {
      if (vm.onRemove) {
        vm.onRemove({ property: vm.property });
      }
    }

    function changeSortOrderValue () {
      if (vm.onChangeSortOrderValue) {
        vm.onChangeSortOrderValue( {property: vm.property});
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
      onChangeSortOrderValue: '&',
      valServerFieldAlias: '@',
      valServerFieldLabel: '@',
      valTabAlias: '@'
    },
    controllerAs: 'vm',
    controller: umbContentTypePropertyController
  };

  angular.module('umbraco.directives').component('umbContentTypeProperty', umbContentTypePropertyComponent);

})();
