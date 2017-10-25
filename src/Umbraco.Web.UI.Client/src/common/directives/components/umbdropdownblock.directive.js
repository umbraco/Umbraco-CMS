/**
@ngdoc directive
@name umbraco.directives.directive:umbDropdownBlock
@restrict E
@scope

@description
Use this directive to render a dropdown object.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-dropdown-block title="This is the title" circle="true" percentage="100">
            // Content of dropdown
        </umb-dropdown-block>

    </div>
</pre>

<h3>Example with {@link umbraco.directives.directive:umbDropdownBlockItem umbDropdownBlockItem}</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-dropdown-block
            ng-repeat="(key,value) in vm.items"
            title="key"
            circle="true" 
            percentage="100">

                <umb-dropdown-block-item
                    on-start="vm.toggle()"
                    tick="true"
                    name="title"
                    completed="true"
                    ng-repeat="item in value">
                </umb-dropdown-block-item>

        </umb-dropdown-block>

    </div>
</pre>

<h3>Use in combination with:</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbDropdownBlockItem umbDropdownBlockItem}</li>
</ul>

@param {string} title (<code>attrbute</code>): Custom title text.
@param {boolean} circle (<code>attrbute</code>): Decides, if the progress circle is displayed or not.
@param {string} percentage (<code>attrbute</code>): A number which defines the progress circle.
**/


(function () {
    'use strict';

    function DropdownBlockDirective(tourService) {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-dropdown-block.html',
            scope: {
                circle: "=",
                title: "=",
                percentage: "@"
            }
        };

        return directive;
    
    }

    angular.module('umbraco.directives').directive('umbDropdownBlock', DropdownBlockDirective);

})();