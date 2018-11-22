(function() {
   'use strict';

   function EditorNavigationDirective() {

      function link(scope, el, attr, ctrl) {

         scope.showNavigation = true;

         scope.clickNavigationItem = function(selectedItem) {
            setItemToActive(selectedItem);
            runItemAction(selectedItem);
         };

         function runItemAction(selectedItem) {
            if (selectedItem.action) {
               selectedItem.action(selectedItem);
            }
         }

         function setItemToActive(selectedItem) {
            // set all other views to inactive
            if (selectedItem.view) {

               for (var index = 0; index < scope.navigation.length; index++) {
                  var item = scope.navigation[index];
                  item.active = false;
               }

               // set view to active
               selectedItem.active = true;

            }
         }

         function activate() {

            // hide navigation if there is only 1 item
            if (scope.navigation.length <= 1) {
               scope.showNavigation = false;
            }

         }

         activate();

      }

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-navigation.html',
         scope: {
            navigation: "="
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives.html').directive('umbEditorNavigation', EditorNavigationDirective);

})();
