/**
@ngdoc directive
@name umbraco.directives.directive:umbFolderGrid
@restrict E
@scope

@description
Use this directive to generate a list of folders presented as a flexbox grid.

<h3>Markup example</h3>
<pre>
    <div ng-controller="Umbraco.Controller as vm">
        <umb-folder-grid
            ng-if="vm.folders"
            folders="vm.folders"
            on-click="vm.clickFolder"
            on-select="vm.selectFolder">
        </umb-folder-grid>
    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller(myService) {

            var vm = this;
            vm.folders = [];

            vm.clickFolder = clickFolder;
            vm.selectFolder = selectFolder;

            myService.getFolders().then(function(folders){
                vm.folders = folders;
            });

            function clickFolder(folder){
                // do magic here
            }

            function selectFolder(folder, event, index) {
                // set folder.selected = true; to select the folder
                // do magic here
            }

        }

        angular.module("umbraco").controller("Umbraco.Controller", Controller);
    })();
</pre>

@param {array} folders (<code>binding</code>): Array of folders
@param {callback=} onClick (<code>binding</code>): Callback method to handle click events on the folder.
    <h3>The callback returns:</h3>
    <ul>
        <li><code>folder</code>: The selected folder</li>
    </ul>
@param {callback=} onSelect (<code>binding</code>): Callback method to handle click events on the checkmark icon.
    <h3>The callback returns:</h3>
    <ul>
        <li><code>folder</code>: The selected folder</li>
        <li><code>$event</code>: The select event</li>
        <li><code>$index</code>: The folder index</li>
    </ul>
**/

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
