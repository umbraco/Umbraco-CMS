(function() {
  'use strict';

  function BreadcrumbsDirective() {

    var directive = {
      restrict: 'E',
      replace: true,
      templateUrl: 'views/components/editor/umb-breadcrumbs.html',
      scope: {
        ancestors: "="
      }
    };

    return directive;

  }

  angular.module('umbraco.directives').directive('umbBreadcrumbs', BreadcrumbsDirective);

})();
