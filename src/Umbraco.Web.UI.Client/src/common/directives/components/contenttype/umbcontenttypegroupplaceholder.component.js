(function () {
  'use strict';

  /**
   * A component to render the content type group placeholder
   */
  
  function umbContentTypeGroupPlaceholderController() {

      const vm = this;

      vm.click = click;

      function click () {
        if (vm.onClick) {
          vm.onClick();
        }
      }

  }

  const umbContentTypeGroupPlaceholderComponent = {
      templateUrl: 'views/components/contenttype/umb-content-type-group-placeholder.html',
      controllerAs: 'vm',
      bindings: {
        onClick: '&'
      },
      controller: umbContentTypeGroupPlaceholderController
  };

  angular.module('umbraco.directives').component('umbContentTypeGroupPlaceholder', umbContentTypeGroupPlaceholderComponent);

})();
