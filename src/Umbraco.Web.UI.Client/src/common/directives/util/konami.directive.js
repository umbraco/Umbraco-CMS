/**
 * Konami Code directive for AngularJS
 * @version v0.0.1
 * @license MIT License, http://www.opensource.org/licenses/MIT
 */

angular.module('umbraco.directives')
  .directive('konamiCode', ['$document', function ($document) {
      var konamiKeysDefault = [38, 38, 40, 40, 37, 39, 37, 39, 66, 65];

      return {
          restrict: 'A',
          link: function (scope, element, attr) {

              if (!attr.konamiCode) {
                  throw ('Konami directive must receive an expression as value.');
              }

              // Let user define a custom code.
              var konamiKeys = attr.konamiKeys || konamiKeysDefault;
              var keyIndex = 0;

              /**
               * Fired when konami code is type.
               */
              function activated() {
                  if ('konamiOnce' in attr) {
                      stopListening();
                  }
                  // Execute expression.
                  scope.$eval(attr.konamiCode);
              }

              /**
               * Handle keydown events.
               */
              function keydown(e) {
                  if (e.keyCode === konamiKeys[keyIndex++]) {
                      if (keyIndex === konamiKeys.length) {
                          keyIndex = 0;
                          activated();
                      }
                  } else {
                      keyIndex = 0;
                  }
              }

              /**
               * Stop to listen typing.
               */
              function stopListening() {
                  $document.off('keydown', keydown);
              }

              // Start listening to key typing.
              $document.on('keydown', keydown);

              // Stop listening when scope is destroyed.
              scope.$on('$destroy', stopListening);
          }
      };
  }]);