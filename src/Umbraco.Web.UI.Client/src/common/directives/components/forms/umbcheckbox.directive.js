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
@param {string} text Set up to two texts for the checkbox label.
@param {string} localize Set up to two localization keys seperated by a pipe (Optional).
@param {string} nodetext Pass a current node so we can have {{localizedText1}} {{nodeName}} {{localizedText2}}


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
                text: "@",
                localize: "@",
                nodetext: "=",
                required: "="
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbCheckbox', CheckboxDirective);

})();
