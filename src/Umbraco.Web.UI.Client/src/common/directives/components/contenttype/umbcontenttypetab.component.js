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
    vm.clickComposition = clickComposition;

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
        vm.onChangeName({ key: vm.tab.key, name: vm.tab.name });
      }
    }
    function clickComposition (documentTypeId) {
      if (vm.onClickComposition) {
        vm.onClickComposition({documentTypeId: documentTypeId});
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
      onClickComposition: '&?',
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
