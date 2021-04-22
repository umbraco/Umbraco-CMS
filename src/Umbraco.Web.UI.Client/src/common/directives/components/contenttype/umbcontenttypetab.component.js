(function () {
  'use strict';

  /**
   * A component to render the content type tab
   */
  
  function umbContentTypeTabController(editorService) {

    const vm = this;

    vm.removePromptIsVisible = false;

    vm.click = click;
    vm.removeTab = removeTab;
    vm.togglePrompt = togglePrompt;
    vm.hidePrompt = hidePrompt;
    vm.whenFocusName = whenFocusName;
    vm.whenFocus = whenFocus;
    vm.changeSortOrderValue = changeSortOrderValue;
    vm.openIconPicker = openIconPicker;

    function togglePrompt () {
      vm.removePromptIsVisible = !vm.removePromptIsVisible;
    }

    function hidePrompt () {
      vm.removePromptIsVisible = false;
    }

    function click () {
      if (vm.onClick) {
        vm.onClick({ tab: vm.tab });
      }
    }

    function removeTab () {
      if (vm.onRemove) {
        vm.onRemove({ tab: vm.tab });
        vm.removePromptIsVisible = false;
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

    function openIconPicker () {
      const iconPicker = {
        icon: vm.tab.icon && vm.tab.icon.split(' ')[0],
        color: vm.tab.icon && vm.tab.icon.split(' ')[1],
        submit: function (model) {
          if (vm.onChangeIcon) {
            vm.onChangeIcon( {icon: model.icon, color: model.color});
          }
          vm.tabIconForm.$setDirty();
          editorService.close();
        },
        close: function () {
          editorService.close();
        }
      };

      editorService.iconPicker(iconPicker);
    }
  }

  const umbContentTypeTabComponent = {
    templateUrl: 'views/components/contenttype/umb-content-type-tab.html',
    controllerAs: 'vm',
    transclude: true,
    bindings: {
      tab: '<',
      onClick: '&',
      isOpen: '<',
      allowRemove: '<',
      onRemove: '&',
      sorting: '<',
      onFocusName: '&',
      onFocus: '&',
      onChangeSortOrderValue: '&',
      onChangeIcon: '&'
    },
    controller: umbContentTypeTabController
  };

  angular.module('umbraco.directives').component('umbContentTypeTab', umbContentTypeTabComponent);
})();
