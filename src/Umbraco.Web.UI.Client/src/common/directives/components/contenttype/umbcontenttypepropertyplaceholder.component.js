(function () {
  'use strict';

  /**
   * A component to render the content type property
   */
  
  function umbContentTypePropertyPlaceholderController() {

      const vm = this;

      vm.click = click;
      vm.whenFocus = whenFocus;

      function click ($event) {
        if (vm.onClick) {
            vm.onClick({$event})
        }
      }

      function whenFocus () {
        if (vm.onFocus) {
          vm.onFocus()
        }
      }
  }

  const umbContentTypePropertyPlaceholderComponent = {
      templateUrl: 'views/components/contenttype/umb-content-type-property-placeholder.html',
      bindings: {
          onClick: "&",
          onFocus: "&",
          focus: "<"
      },
      controllerAs: 'vm',
      controller: umbContentTypePropertyPlaceholderController
  };

  angular.module('umbraco.directives').component('umbContentTypePropertyPlaceholder', umbContentTypePropertyPlaceholderComponent);

})();
