(function() {
   'use strict';

   function EditorSubHeaderContentRightDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/subheader/umb-editor-sub-header-content-right.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorSubHeaderContentRight', EditorSubHeaderContentRightDirective);

})();
