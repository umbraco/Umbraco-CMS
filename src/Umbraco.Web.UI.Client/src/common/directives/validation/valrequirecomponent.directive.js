(function() {
  'use strict';

  function valRequireComponentDirective() {

    function link(scope, el, attr, ngModel) {

      var unbindModelWatcher = scope.$watch(function () {
        return ngModel.$modelValue;
      }, function(newValue) {

        if(newValue === undefined || newValue === null || newValue === "") {
          ngModel.$setValidity("valRequiredComponent", false);
        } else {
          ngModel.$setValidity("valRequiredComponent", true);
        }

      });

      // clean up
      scope.$on('$destroy', function(){
        unbindModelWatcher();
      });

    }

    var directive = {
      require: 'ngModel',
      restrict: "A",
      link: link
    };

    return directive;
  }

    angular.module('umbraco.directives').directive('valRequireComponent', valRequireComponentDirective);

})();
