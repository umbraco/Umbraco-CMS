(function() {
   'use strict';

   function EditorSubHeaderDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/subheader/umb-editor-sub-header.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorSubHeader', EditorSubHeaderDirective);

})();
