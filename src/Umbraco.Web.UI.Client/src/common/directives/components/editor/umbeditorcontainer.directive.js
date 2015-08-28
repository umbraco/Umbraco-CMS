(function() {
   'use strict';

   function EditorContainerDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-container.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorContainer', EditorContainerDirective);

})();
