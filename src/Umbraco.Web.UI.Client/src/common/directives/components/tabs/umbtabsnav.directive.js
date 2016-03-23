(function() {
  'use strict';

  function UmbTabsNavDirective($timeout) {

    function link(scope, el, attr) {

      function activate() {

        $timeout(function () {

          //use bootstrap tabs API to show the first one
          el.find("a:first").tab('show');

          //enable the tab drop
          el.tabdrop();

        });

      }

      var unbindModelWatch = scope.$watch('model', function(newValue, oldValue){

        activate();

      });


      scope.$on('$destroy', function () {

          //ensure to destroy tabdrop (unbinds window resize listeners)
          el.tabdrop("destroy");

          unbindModelWatch();

      });

    }

    var directive = {
      restrict: "E",
      replace: true,
      templateUrl: "views/components/tabs/umb-tabs-nav.html",
      scope: {
        model: "=",
        tabdrop: "="
      },
      link: link
    };

    return directive;
  }

  angular.module('umbraco.directives').directive('umbTabsNav', UmbTabsNavDirective);

})();
