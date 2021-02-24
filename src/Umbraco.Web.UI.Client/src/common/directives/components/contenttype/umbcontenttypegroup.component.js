(function () {
  'use strict';

  /**
   * A component to render the content type group
   */
  
  function umbContentTypeGroupController() {

      const vm = this;

      vm.updateName = updateName;

      function updateName (group) {
        if (vm.onUpdateName) {
          vm.onUpdateName({ group });
        }
      }

  }

  const umbContentTypeGroupComponent = {
      templateUrl: 'views/components/contenttype/umb-content-type-group.html',
      controllerAs: 'vm',
      transclude: true,
      bindings: {
        group: "<",
        allowName: "<",
        onUpdateName: "&"
      },
      controller: umbContentTypeGroupController
  };

  angular.module('umbraco.directives').component('umbContentTypeGroup', umbContentTypeGroupComponent);

})();
