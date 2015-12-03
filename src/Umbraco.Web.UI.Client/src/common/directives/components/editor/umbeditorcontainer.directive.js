(function() {
   'use strict';

   function EditorContainerDirective(overlayHelper) {

      function link(scope, el, attr, ctrl) {

         scope.numberOfOverlays = 0;

         scope.$watch(function(){
            return overlayHelper.getNumberOfOverlays();
         }, function (newValue) {
            scope.numberOfOverlays = newValue;
         });

      }

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-container.html',
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorContainer', EditorContainerDirective);

})();
