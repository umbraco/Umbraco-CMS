(function () {
  'use strict';

  /**
   * A component to render the content type tab
   */

  function umbContentTypeTabController($timeout) {

    const vm = this;

    vm.compositionLabelIsVisible = false;

    vm.click = click;
    vm.removeTab = removeTab;
    vm.whenFocusName = whenFocusName;
    vm.whenFocus = whenFocus;
    vm.changeSortOrderValue = changeSortOrderValue;
    vm.changeName = changeName;
    vm.clickComposition = clickComposition;
    vm.mouseenter = mouseenter;
    vm.mouseleave = mouseleave;
    
    let timeout = null;

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

    function clickComposition(contentTypeId) {
      if (vm.onClickComposition) {
        vm.onClickComposition({ contentTypeId: contentTypeId});
      }
    }

    function mouseenter () {
      if (vm.tab.inherited) {
        vm.compositionLabelIsVisible = true;
        $timeout.cancel(timeout);
      }
    }

    function mouseleave () {
      if (vm.tab.inherited) {
        timeout = $timeout(() => {
          vm.compositionLabelIsVisible = false;
        }, 300);
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
