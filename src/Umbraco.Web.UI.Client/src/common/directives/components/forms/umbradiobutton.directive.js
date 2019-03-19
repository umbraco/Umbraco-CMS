/**
@ngdoc directive
@name umbraco.directives.directive:umbRadiobutton
@restrict E
@scope

@description
<b>Added in Umbraco version 7.14.0</b> Use this directive to render an umbraco radio button.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-radiobutton
            name="radiobuttonlist"
            value="{{key}}"
            model="true"
            text="{{text}}">
        </umb-radiobutton>

    </div>
</pre>

@param {boolean} model Set to <code>true</code> or <code>false</code> to set the radiobutton to checked or unchecked.
@param {string} value Set the value of the radiobutton.
@param {string} name Set the name of the radiobutton.
@param {string} text Set the text for the radiobutton label.
@param {boolean} disabled Set the radiobutton to be disabled.
@param {boolean} required Set the radiobutton to be required.

**/

(function () {
    'use strict';

    function RadiobuttonDirective() {
        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/forms/umb-radiobutton.html',
            scope: {
                model: "=",
                value: "@",
                name: "@",
                text: "@",
                disabled: "=",
                required: "="
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbRadiobutton', RadiobuttonDirective);

})();
