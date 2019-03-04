/**
@ngdoc directive
@name umbraco.directives.directive:umbCheckbox
@restrict E
@scope

@description
<b>Added in Umbraco version 7.14.0</b> Use this directive to render an umbraco checkbox.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-checkbox
            name="checkboxlist"
            value="{{key}}"
            model="true"
            text="{{text}}">
        </umb-checkbox>

    </div>
</pre>

@param {boolean} model Set to <code>true</code> or <code>false</code> to set the checkbox to checked or unchecked.
@param {string} value Set the value of the checkbox.
@param {string} name Set the name of the checkbox.
@param {string} text Set the text for the checkbox label.


**/

(function () {
    'use strict';

    function CheckboxDirective() {
        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/forms/umb-checkbox.html',
            scope: {
                model: "=",
                value: "@",
                name: "@",
                text: "@"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbCheckbox', CheckboxDirective);

})();



