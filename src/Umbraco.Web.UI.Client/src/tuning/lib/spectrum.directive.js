angular.module('spectrumcolorpicker', [])
  .directive('spectrum', function () {
      return {
          restrict: 'E',
          transclude: true,
          scope: {
              colorselected: '='
          },
          link: function (scope, $element) {

              $element.find("input").spectrum({
                  color: scope.colorselected,
                  preferredFormat: "rgb",
                  showAlpha: true,
                  showInput: true,
                  change: function (color) {
                      scope.colorselected = color.toRgbString();
                      scope.$apply();
                  },
                  move: function (color) {
                  }
              });

              scope.$watch('colorselected', function () {
                  $element.find("input").spectrum("set", scope.colorselected);
              }, true);

          },
          template:
          '<div><input type=\'text\' ng-model=\'colorselected\' /></div>',
          replace: true
      };
  })