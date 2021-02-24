(function () {
  'use strict';

  /**
   * A component to render the content type property
   */
  
  function umbContentTypePropertyPlaceholderController() {

      const vm = this;

      vm.click = click;

      function click ($event) {
        if (vm.onClick) {
            vm.onClick({$event})
        }
      }

  }

  const umbContentTypePropertyPlaceholderComponent = {
      templateUrl: 'views/components/contenttype/umb-content-type-property-placeholder.html',
      bindings: {
          onClick: "&"
      },
      controllerAs: 'vm',
      controller: umbContentTypePropertyPlaceholderController
  };

  angular.module('umbraco.directives').component('umbContentTypePropertyPlaceholder', umbContentTypePropertyPlaceholderComponent);

})();
