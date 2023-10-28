(function () {
  'use strict';

  /**
   * A component to render the content type group
   */

  function umbContentTypeGroupController() {

    const vm = this;

    vm.updateName = updateName;
    vm.removeGroup = removeGroup;
    vm.whenNameFocus = whenNameFocus;
    vm.whenFocus = whenFocus;
    vm.changeSortOrderValue = changeSortOrderValue;
    vm.clickComposition = clickComposition;

    function updateName (group) {
      if (vm.onUpdateName) {
        vm.onUpdateName({ group });
      }
    }

    function removeGroup () {
      if (vm.onRemove) {
        vm.onRemove({ group: vm.group });
      }
    }

    function whenNameFocus () {
      if (vm.onNameFocus) {
        vm.onNameFocus();
      }
    }

    function whenFocus () {
      if (vm.onFocus) {
        vm.onFocus();
      }
    }

    function changeSortOrderValue () {
      if (vm.onChangeSortOrderValue) {
        vm.onChangeSortOrderValue( {group: vm.group});
      }
    }
    function clickComposition (contentTypeId) {
      if (vm.onClickComposition) {
        vm.onClickComposition({contentTypeId: contentTypeId});
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
      onNameFocus: '&',
      onFocus: '&',
      onChangeSortOrderValue: '&',
      valServerFieldName: '@',
      valTabAlias: "@",
      onClickComposition: '&?'
    },
    controller: umbContentTypeGroupController
  };

  angular.module('umbraco.directives').component('umbContentTypeGroup', umbContentTypeGroupComponent);
})();
