(function() {
   'use strict';

   function EditorMenuDirective($injector, $timeout, treeService, navigationService, umbModelMapper, appState) {

      function link(scope, el, attr, ctrl) {

          scope.isOpen = false;

         //adds a handler to the context menu item click, we need to handle this differently
         //depending on what the menu item is supposed to do.
         scope.executeMenuItem = function (action) {
             navigationService.executeMenuAction(action, scope.currentNode, scope.currentSection);
         };

         //callback method to go and get the options async
         scope.getOptions = function () {

             if (!scope.currentNode) {
                 return;
             }

             //when the options item is selected, we need to set the current menu item in appState (since this is synonymous with a menu)
             appState.setMenuState("currentNode", scope.currentNode);

             if (!scope.actions) {
                 treeService.getMenu({ treeNode: scope.currentNode })
                     .then(function (data) {
                         scope.actions = data.menuItems;
                         scope.actions[0].hasFocus = true;
                     });
             } else {
                scope.actions[0].hasFocus = true;
             }
         };

         scope.toggleDropdown = function() {
            scope.isOpen = !scope.isOpen;
            if(scope.isOpen) {
                scope.getOptions();
            }
         };

         scope.closeDropdown = function() {
            scope.isOpen = false;
         };

         

      }

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-menu.html',
         link: link,
         scope: {
            currentNode: "=",
            currentSection: "@"
         }
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorMenu', EditorMenuDirective);

})();
