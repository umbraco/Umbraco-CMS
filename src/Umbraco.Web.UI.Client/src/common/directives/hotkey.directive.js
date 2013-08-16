/**
* @ngdoc directive
* @name umbraco.directives.directive:headline
**/
angular.module("umbraco.directives")
  .directive('hotkey', function ($window, keyboardService, $log) {
      return function (scope, el, attrs) {

          var keyCombo = attrs["hotkey"];
          $log.log(keyCombo);

          keyboardService.bind(keyCombo, function() {
              var element = $(el);

              $log.log(element);

              if(element.is("a,button,input[type='button'],input[type='submit']")){
                element.click();
                $log.log("click");

              }else{
                element.focus();
                $log.log("focus");
              }
          });
          
      };
  });