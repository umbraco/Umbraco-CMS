(function() {
   'use strict';

   function FolderGridDirective() {

      function link(scope, el, attr, ctrl) {

         scope.clickFolder = function(folder, $event, $index) {
            if(scope.onClick) {
               scope.onClick(folder, $event, $index);
            }
         };

         scope.clickFolderName = function(folder, $event, $index) {
            if(scope.onClickName) {
               scope.onClickName(folder, $event, $index);
               $event.stopPropagation();
            }
         };

      }

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/umb-folder-grid.html',
         scope: {
            folders: '=',
            onClick: "=",
            onClickName: "="
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbFolderGrid', FolderGridDirective);

})();
