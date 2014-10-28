
/*********************************************************************************************************/
/* spectrum color picker directive */
/*********************************************************************************************************/

angular.module('spectrumcolorpicker', [])
  .directive('spectrum', function () {
      return {
          restrict: 'E',
          transclude: true,
          scope: {
              colorselected: '=',
              setColor: '=',
              flat: '=',
              showPalette: '='
          },
          link: function (scope, $element) {

              var initColor;

              $element.find("input").spectrum({
                  color: scope.colorselected,
                  allowEmpty: true,
                  preferredFormat: "hex",
                  showAlpha: true,
                  showInput: true,
                  flat: scope.flat,
                  localStorageKey: "spectrum.panel",
                  showPalette: scope.showPalette,
                  palette: [],
                  change: function (color) {

                      if (color) {
                          scope.colorselected = color.toRgbString();
                      }
                      else {
                          scope.colorselected = '';
                      }
                      scope.$apply();
                  },
                  move: function (color) {
                      scope.colorselected = color.toRgbString();
                      scope.$apply();
                  },
                  beforeShow: function (color) {
                      initColor = angular.copy(scope.colorselected);
                      $(this).spectrum("container").find(".sp-cancel").click(function (e) {
                          scope.colorselected = initColor;
                          scope.$apply();
                      });
                  },

              });

              scope.$watch('setcolor', function (setColor) {
                  if (scope.$eval(setColor) === true) {
                      $element.find("input").spectrum("set", scope.colorselected);
                  }
              }, true);

          },
          template:
          '      <div class="spectrumcolorpicker"><div class="real-color-preview" style="background-color:{{colorselected}}"></div><input type=\'text\' ng-model=\'colorselected\' /></div>',
          replace: true
      };
  })