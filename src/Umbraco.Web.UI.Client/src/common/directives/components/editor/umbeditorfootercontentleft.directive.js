(function() {
   'use strict';

   function EditorFooterContentLeftDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-footer-content-left.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorFooterContentLeft', EditorFooterContentLeftDirective);

})();
