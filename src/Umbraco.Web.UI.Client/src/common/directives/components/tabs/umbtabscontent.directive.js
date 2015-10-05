(function() {
  'use strict';

  function UmbTabsContentDirective() {

    function link(scope, el, attr, ctrl) {

      scope.view = attr.view;

   }

    var directive = {
      restrict: "E",
      replace: true,
      transclude: 'true',
      templateUrl: "views/components/tabs/umb-tabs-content.html",
      link: link
    };

    return directive;
  }

  angular.module('umbraco.directives').directive('umbTabsContent', UmbTabsContentDirective);

})();
