/**
@ngdoc directive
@name umbraco.directives.directive:umbDropdown
@restrict E
@scope

@description
<b>Added in versions 7.7.0</b>: Use this component to render a dropdown menu.

<h3>Markup example</h3>
<pre>
    <div ng-controller="MyDropdown.Controller as vm">

        <div style="position: relative;">

            <umb-button
                type="button"
                label="Toggle dropdown"
                action="vm.toggle()">
            </umb-button>

            <umb-dropdown ng-if="vm.dropdownOpen" on-close="vm.close()" umb-keyboard-list>
                <umb-dropdown-item
                    ng-repeat="item in vm.items">
                    <button type="button" class="btn-reset" ng-click="vm.select(item)">{{ item.name }}</button>
                </umb-dropdown-item>
            </umb-dropdown>

        </div>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller() {

            var vm = this;

            vm.dropdownOpen = false;
            vm.items = [
                { "name": "Item 1" },
                { "name": "Item 2" },
                { "name": "Item 3" }
            ];

            vm.toggle = toggle;
            vm.close = close;
            vm.select = select;

            function toggle() {
                vm.dropdownOpen = true;
            }

            function close() {
                vm.dropdownOpen = false;
            }

            function select(item) {
                // Do your magic here
            }

        }

        angular.module("umbraco").controller("MyDropdown.Controller", Controller);
    })();
</pre>

<h3>Use in combination with</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbDropdownItem umbDropdownItem}</li>
    <li>{@link umbraco.directives.directive:umbKeyboardList umbKeyboardList}</li>
</ul>

@param {callback} onClose Callback when the dropdown menu closes. When you click outside or press esc.

**/

(function() {
    'use strict';

    function umbDropdown($document) {

        function link(scope, element, attr, ctrl) {

            scope.close = function() {
                if (scope.onClose) {
                    scope.onClose();
                }
            };

            // Handle keydown events
            function keydown(event) {
                // press escape
                if(event.keyCode === 27) {
                    scope.onClose();
                }
            }

            // Stop to listen typing.
            function stopListening() {
                $document.off('keydown', keydown);
            }

            // Start listening to key typing.
            $document.on('keydown', keydown);

            // Stop listening when scope is destroyed.
            scope.$on('$destroy', stopListening);

        }

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-dropdown.html',
            scope: {
                onClose: "&"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbDropdown', umbDropdown);

})();
