angular.module("umbraco.directives")
  .directive('selectOnFocus', function () {
    return function (scope, el, attrs) {
        $(el).bind("click", function(){
          this.select();
        });
    };
  });