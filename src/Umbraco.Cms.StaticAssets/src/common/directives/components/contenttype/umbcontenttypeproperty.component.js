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
    vm.clickComposition = clickComposition;

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

    function clickComposition(contentTypeId) {
      if (vm.onClickComposition) {
        vm.onClickComposition({ contentTypeId: contentTypeId });
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
      onClickComposition: '&?',
      valServerFieldAlias: '@',
      valServerFieldLabel: '@',
      valTabAlias: '@'
    },
    controllerAs: 'vm',
    controller: umbContentTypePropertyController
  };

  angular.module('umbraco.directives').component('umbContentTypeProperty', umbContentTypePropertyComponent);

})();
