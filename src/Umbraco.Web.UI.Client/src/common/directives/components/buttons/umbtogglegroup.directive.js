/**
@ngdoc directive
@name umbraco.directives.directive:umbToggleGroup
@restrict E
@scope

@description
Use this directive to render a group of toggle buttons.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-toggle-group
            items="vm.items"
            on-click="vm.toggle(item)">
        </umb-toggle-group>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller() {

            var vm = this;
            vm.toggle = toggle;

            function toggle(item) {
                if(item.checked) {
                    // do something if item is checked
                }
                else {
                    // do something else if item is unchecked
                }
            }

            function init() {
                vm.items = [{
                    name: "Item 1",
                    description: "Item 1 description",
                    checked: false,
                    disabled: false
                }, {
                    name: "Item 2",
                    description: "Item 2 description",
                    checked: true,
                    disabled: true
                }];
            }

            init();
        }

        angular.module("umbraco").controller("My.Controller", Controller);

    })();
</pre>

@param {Array} items The items to list in the toggle group
@param {callback} onClick The function which should be called when the toggle is clicked for one of the items.

**/

(function () {
    'use strict';

    function ToggleGroupDirective() {

        function link(scope, el, attr, ctrl) {
            for(let i = 0; i < scope.items.length; i++) {
                scope.items[i].inputId = "umb-toggle-group-item_" + String.CreateGuid();
            }
            scope.change = function(item) {
                if (item.disabled) {
                    return;
                }

                item.checked = !item.checked;
                if(scope.onClick) {
                    scope.onClick({'item': item});
                }
            };

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/buttons/umb-toggle-group.html',
            scope: {
                items: "=",
                onClick: "&"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbToggleGroup', ToggleGroupDirective);

})();
