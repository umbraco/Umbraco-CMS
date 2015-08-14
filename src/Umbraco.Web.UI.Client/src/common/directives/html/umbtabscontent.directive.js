(function() {
  'use strict';

  function UmbTabsContentDirective() {

    var directive = {
      restrict: "E",
      replace: true,
      transclude: 'true',
      templateUrl: "views/directives/umb-tabs-content.html"
    };

    return directive;
  }

  angular.module('umbraco.directives').directive('umbTabsContent', UmbTabsContentDirective);

})();
