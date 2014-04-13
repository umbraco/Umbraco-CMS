/**
* @ngdoc directive
* @name umbraco.directives.directive:headline
**/
angular.module("umbraco.directives")
  .directive('hotkey', function ($window, keyboardService, $log) {

      return function (scope, el, attrs) {
          
          //support data binding
    
          var keyCombo = scope.$eval(attrs["hotkey"]);
          if (!keyCombo) {
              keyCombo = attrs["hotkey"];
          }

          keyboardService.bind(keyCombo, function() {
              var element = $(el);

              if(element.is("a,button,input[type='button'],input[type='submit']") && !element.is(':disabled') ){
                element.click();
              }else{
                element.focus();
              }
          });
          
      };
  });