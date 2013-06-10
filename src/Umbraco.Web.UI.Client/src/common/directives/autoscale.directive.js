angular.module("umbraco.directives")
  .directive('autoScale', function ($window) {
    return function (scope, el, attrs) {

      var totalOffset = 0;
      var offsety = parseInt(attrs.autoScale, 10);
      var window = angular.element($window);
      if (offsety !== undefined){
        totalOffset += offsety;
      }

      setTimeout(function () {
        el.height(window.height() - (el.offset().top + totalOffset));
      }, 300);

      window.bind("resize", function () {
        el.height(window.height() - (el.offset().top + totalOffset));
      });

    };
  });