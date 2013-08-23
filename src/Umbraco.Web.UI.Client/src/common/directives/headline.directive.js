/**
* @ngdoc directive
* @name umbraco.directives.directive:headline

angular.module("umbraco.directives")
  .directive('headline', function ($window) {
      return function (scope, el, attrs) {

          var h1 = $("<h1 class='umb-headline-editor'></h1>").hide();
          el.parent().prepend(h1);
          el.addClass("umb-headline-editor");



          if (el.val() !== '') {
              el.hide();
              h1.text(el.val());
              h1.show();
          } else {
              el.focus();
          }


          el.on("blur", function () {
              //Don't hide the input field if there is no value in it
              if (el.val() !== '') {
                  el.hide();
                  h1.html(el.val()).show();
              }
          });

          h1.on("click", function () {
              h1.hide();
              el.show().focus();
          });
      };
  });
*/

angular.module('umbraco.directives').directive('headline', function() {
    return {
      restrict: 'E',
      require: '?ngModel',
      transclude: false,
      template: '<div class="umb-headline-editor-wrapper"><h1 class="umb-headline-editor">{{ngModel}}</h1><input type="text"></div>',

      link: function(scope, element, attrs, ngModel) {
        
        function read() {
          ngModel.$setViewValue(editor.getValue());
          textarea.val(editor.getValue());
        }

        var input = $(element).find('input');
        var h1 = $(element).find('h1');
        input.hide();
        
        if (!ngModel)
        {
          return; // do nothing if no ngModel
        }

        ngModel.$render = function() {
          var value = ngModel.$viewValue || '';
          input.val(value);
          h1.text(value);

          if(value === ''){
            input.show();
            h1.hide();
          }
        };


        input.on("blur", function () {
            //Don't hide the input field if there is no value in it
            var val = input.val() || "empty";
            input.hide();
            h1.text(val);
            h1.show();
        });

        h1.on("click", function () {
            h1.hide();
            input.show().focus();
        });
      } 
    };
  });  