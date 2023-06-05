/**
@ngdoc directive
@name umbraco.directives.directive:umbContentGrid
@restrict E
@scope

@description
Use this directive to generate a list of content items presented as a flexbox grid.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-content-grid
            content="vm.contentItems"
            content-properties="vm.includeProperties"
            on-click="vm.selectItem"
            on-click-name="vm.clickItem">
        </umb-content-grid>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller() {

            var vm = this;
            vm.contentItems = [
                {
                    "name": "Cape",
                    "published": true,
                    "icon": "icon-document",
                    "updateDate": "15-02-2016",
                    "owner": "Mr. Batman",
                    "selected": false
                },
                {
                    "name": "Utility Belt",
                    "published": true,
                    "icon": "icon-document",
                    "updateDate": "15-02-2016",
                    "owner": "Mr. Batman",
                    "selected": false
                },
                {
                    "name": "Cave",
                    "published": true,
                    "icon": "icon-document",
                    "updateDate": "15-02-2016",
                    "owner": "Mr. Batman",
                    "selected": false
                }
            ];
            vm.includeProperties = [
                {
                  "alias": "updateDate",
                  "header": "Last edited"
                },
                {
                  "alias": "owner",
                  "header": "Created by"
                }
            ];

            vm.clickItem = clickItem;
            vm.selectItem = selectItem;


            function clickItem(item, $event, $index){
                // do magic here
            }

            function selectItem(item, $event, $index) {
                // set item.selected = true; to select the item
                // do magic here
            }

        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

@param {array} content (<code>binding</code>): Array of content items.
@param {array=} contentProperties (<code>binding</code>): Array of content item properties to include in the item. If left empty the item will only show the item icon and name.
@param {callback=} onClick (<code>binding</code>): Callback method to handle click events on the content item.
    <h3>The callback returns:</h3>
    <ul>
        <li><code>item</code>: The clicked item</li>
        <li><code>$event</code>: The select event</li>
        <li><code>$index</code>: The item index</li>
    </ul>
@param {callback=} onClickName (<code>binding</code>): Callback method to handle click events on the checkmark icon.
    <h3>The callback returns:</h3>
    <ul>
        <li><code>item</code>: The selected item</li>
        <li><code>$event</code>: The select event</li>
        <li><code>$index</code>: The item index</li>
    </ul>
**/

(function() {
   'use strict';

   function ContentGridDirective() {

      function link(scope, el, attr, ctrl) {

         scope.clickItem = function(item, $event, $index) {
            if(scope.allowClick !== false && scope.onClick) {
               scope.onClick(item, $event, $index);
            }
         };

         scope.clickItemName = function(item, $event, $index) {
            if(scope.onClickName && !($event.metaKey || $event.ctrlKey)) {
               scope.onClickName(item, $event, $index);
               $event.preventDefault();
            }
            $event.stopPropagation();
         };

      }

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/umb-content-grid.html',
         scope: {
            content: '=',
            contentProperties: "=",
            allowClick: "<?",
            onClick: "=",
            onClickName: "="
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbContentGrid', ContentGridDirective);

})();
