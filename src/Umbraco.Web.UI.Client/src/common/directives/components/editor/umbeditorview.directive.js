(function() {
   'use strict';

   function EditorViewDirective() {

       function link(scope, el, attr) {

           if(attr.footer) {
               scope.footer = attr.footer;
           }

       }

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-view.html',
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorView', EditorViewDirective);

})();
