(function() {
  'use strict';

  function BlurDirective($parse) {

    return function(scope, element, attr) {
      var fn = $parse(attr['umbBlur']);
      element.bind('blur', function(event) {
        scope.$apply(function() {
          fn(scope, {$event:event});
        });
      });
    };
  }

  angular.module('umbraco.directives').directive('umbBlur', BlurDirective);

})();
