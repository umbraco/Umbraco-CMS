/**
@ngdoc directive
@name umbraco.directives.directive:umbDropdownBlockItem
@restrict E
@scope

@description
Use this directive to render a list for the {@link umbraco.directives.directive:umbDropdownBlock umbDropdownBlock} directive. See documentation for {@link umbraco.directives.directive:umbDropdownBlock umbDropdownBlock} component.

<h3>Markup example</h3>

<pre>
    <div ng-controller="My.Controller as vm">

        <umb-dropdown-block
            ng-repeat="(key,value) in vm.items"
            title="key"
            circle="true" 
            percentage="100">

                <umb-dropdown-block-item
                    ng-repeat="item in value"                
                    name="title"
                    completed="true"
                    tick="true"
                    on-start="vm.toggle()">
                </umb-dropdown-block-item>

        </umb-dropdown-block>

    </div>
</pre>

<h3>Use in combination with:</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbDropdownBlock umbDropdownBlock}</li>
</ul>

@param {string} name (<code>attrbute</code>): Custom text.
@param {callback} onStart (<code>attrbute</code>): Defines what happens when the row is clicked.
@param {boolean} completed (<code>attrbute</code>): Decides, if the tick is gray or green.
@param {boolean} tick (<code>attrbute</code>): Decides, if the tick is displayed or not.
**/


(function () {
    'use strict';

    function DropdownBlockItemDirective() {

        function link(scope, el, attr, ctrl) {

            scope.clickStart = function() {
                if(scope.onStart) {
                    scope.onStart();
                }
            };
      
        }

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-dropdown-block-item.html',
            scope: {
                name: "@",
                tick: "=",
                onStart: "&",
                completed: "@"
            },
            link: link
        };

        return directive;
    
    }

    angular.module('umbraco.directives').directive('umbDropdownBlockItem', DropdownBlockItemDirective);

})();