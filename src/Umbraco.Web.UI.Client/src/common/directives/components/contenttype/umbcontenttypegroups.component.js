(function () {
  'use strict';

  /**
   * A component to render the content type groups
   */
  
  function umbContentTypeGroupsController() {

      const vm = this;

  }

  const umbContentTypeGroupsComponent = {
      templateUrl: 'views/components/contenttype/umb-content-type-groups.html',
      controllerAs: 'vm',
      transclude: true,
      controller: umbContentTypeGroupsController
  };

  angular.module('umbraco.directives').component('umbContentTypeGroups', umbContentTypeGroupsComponent);

})();
