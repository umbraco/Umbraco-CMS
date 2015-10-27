(function() {
   'use strict';

   function FolderGridDirective() {

      function link(scope, el, attr, ctrl) {

         scope.clickFolder = function(folder) {
            if(scope.onClick) {
               scope.onClick(folder);
            }
         };

         scope.selectFolder = function(folder, $event, $index) {
            if(scope.onSelect) {
               scope.onSelect(folder, $event, $index);
            }
            $event.stopPropagation();
         };

      }

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/umb-folder-grid.html',
         scope: {
            folders: '=',
            onSelect: '=',
            onClick: "="
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbFolderGrid', FolderGridDirective);

})();
