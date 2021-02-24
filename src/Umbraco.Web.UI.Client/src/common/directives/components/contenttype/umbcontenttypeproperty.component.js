(function () {
  'use strict';

  /**
   * A component to render the content type property
   */
  
  function umbContentTypePropertyController() {

      const vm = this;

      

  }

  const umbContentTypePropertyComponent = {
      templateUrl: 'views/components/contenttype/umb-content-type-property.html',
      bindings: {
          property: "<",
          compact: "<"
      },
      controllerAs: 'vm',
      controller: umbContentTypePropertyController
  };

  angular.module('umbraco.directives').component('umbContentTypeProperty', umbContentTypePropertyComponent);

})();
