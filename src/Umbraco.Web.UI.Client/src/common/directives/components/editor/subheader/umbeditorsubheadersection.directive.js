(function() {
   'use strict';

   function EditorSubHeaderSectionDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/subheader/umb-editor-sub-header-section.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorSubHeaderSection', EditorSubHeaderSectionDirective);

})();
