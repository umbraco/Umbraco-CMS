/**
@ngdoc directive
@name umbraco.directives.directive:umbDropdown
@restrict E

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

            <umb-dropdown ng-if="vm.dropdownOpen" umb-keyboard-list>
                <umb-dropdown-item
                    ng-repeat="item in vm.items"
                    on-outside-click="vm.close()">
                    <a href="" ng-click="vm.select(item)">{{ item.name }}</a>
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

**/

(function() {
    'use strict';

    function umbDropdown() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-dropdown.html'
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbDropdown', umbDropdown);

})();
