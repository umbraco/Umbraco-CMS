(function () {
  'use strict';

  /**
   * A component to render the content type tab
   */
  
  function umbContentTypeTabController() {

    const vm = this;

    vm.click = click;
    vm.removeTab = removeTab;
    vm.whenFocusName = whenFocusName;
    vm.whenFocus = whenFocus;
    vm.changeSortOrderValue = changeSortOrderValue;
    vm.changeName = changeName;

    function click () {
      if (vm.onClick) {
        vm.onClick({ tab: vm.tab });
      }
    }

    function removeTab () {
      if (vm.onRemove) {
        vm.onRemove({ tab: vm.tab });
      }
    }

    function whenFocusName () {
      if (vm.onFocusName) {
        vm.onFocusName();
      }
    }

    function whenFocus () {
      if (vm.onFocus) {
        vm.onFocus();
      }
    }

    function changeSortOrderValue () {
      if (vm.onChangeSortOrderValue) {
        vm.onChangeSortOrderValue( {tab: vm.tab});
      }
    }

    function changeName () {
      if (vm.onChangeName) {
        vm.onChangeName({ name: vm.tab.name });
      }
    }

  }

  const umbContentTypeTabComponent = {
    templateUrl: 'views/components/contenttype/umb-content-type-tab.html',
    controllerAs: 'vm',
    transclude: true,
    bindings: {
      tab: '<',
      onClick: '&?',
      isOpen: '<?',
      allowRemove: '<?',
      onRemove: '&?',
      sorting: '<?',
      onFocusName: '&?',
      onFocus: '&?',
      onChangeSortOrderValue: '&?',
      allowChangeName: '<?',
      onChangeName: '&?',
      valServerFieldName: '@'
    },
    controller: umbContentTypeTabController
  };

  angular.module('umbraco.directives').component('umbContentTypeTab', umbContentTypeTabComponent);
})();
