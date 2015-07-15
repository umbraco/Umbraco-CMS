/**
* @ngdoc directive
* @name umbraco.directives.directive:headline
**/
angular.module("umbraco.directives")
  .directive('hotkey', function ($window, keyboardService, $log) {

      return function (scope, el, attrs) {

          var options = {};
          var keyCombo = attrs.hotkey;

          if (!keyCombo) {
            //support data binding
            keyCombo = scope.$eval(attrs["hotkey"]);
          }

          // disable shortcuts in input fields if keycombo is 1 character
          if(keyCombo.length === 1) {
            options = {
              inputDisabled: true
            };
          }

          keyboardService.bind(keyCombo, function(){
              var element = $(el);
              if(element.is("a,div,button,input[type='button'],input[type='submit'],input[type='checkbox']") && !element.is(':disabled') ){
                element.click();
              }else{
                element.focus();
              }
          }, options);

          el.on('$destroy', function(){
            keyboardService.unbind(keyCombo);
          });

      };
  });
