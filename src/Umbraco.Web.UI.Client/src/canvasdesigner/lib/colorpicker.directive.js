
/*********************************************************************************************************/
/* spectrum color picker directive */
/*********************************************************************************************************/

angular.module('colorpicker', ['spectrumcolorpicker'])
  .directive('colorpicker', ['dialogService', function (dialogService) {
      return {
          restrict: 'EA',
          scope: {
              ngModel: '='
          },
          link: function (scope, $element) {

              scope.openColorDialog = function () {
                  var config = {
                      template: "colorModal.html",
                      change: function (data) {
                          scope.ngModel = data;
                      },
                      callback: function (data) {
                          scope.ngModel = data;
                      },
                      cancel: function (data) {
                          scope.ngModel = data;
                      },
                      dialogItem: scope.ngModel,
                      scope: scope
                  };

                  dialogService.open(config);
              }

              scope.setColor = false;

              scope.submitAndClose = function () {
                  if (scope.ngModel != "") {
                      scope.setColor = true;
                      scope.submit(scope.ngModel);
                  } else {
                      scope.cancel();
                  }

              };

              scope.cancelAndClose = function () {
                  scope.cancel();
              }

          },
          template:
            '<div>' + 
                '<div class="color-picker-preview" ng-click="openColorDialog()" style="background: {{ngModel}} !important;"></div>' +
                '<script type="text/ng-template" id="colorModal.html">' + 
                    '<div class="modal-header">' + 
                        '<h1>Header</h1>' + 
                    '</div>' + 
                    '<div class="modal-body">' + 
                        '<spectrum colorselected="ngModel" set-color="setColor" flat="true" show-palette="true"></spectrum>' +
                    '</div>' + 
                    '<div class="right">' + 
                        '<a class="btn" href="#" ng-click="cancelAndClose()">Cancel</a>' + 
                        '<a class="btn btn-success" href="#" ng-click="submitAndClose()">Done</a>' + 
                    '</div>' + 
                '</script>' + 
            '</div>',
          replace: true
      };
  }])