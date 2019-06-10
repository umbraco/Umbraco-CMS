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
@param {string} input-id Set the <code>id</code> of the checkbox.
@param {string} value Set the value of the checkbox.
@param {string} name Set the name of the checkbox.
@param {string} text Set the text for the checkbox label.
@param {string} localize Set a dictinary/localization string for the checkbox label
@param {string} server-validation-field Set the <code>val-server-field</code> of the checkbox.
@param {boolean} disabled Set the checkbox to be disabled.
@param {boolean} required Set the checkbox to be required.
@param {string} on-change Callback when the value of the checkbox changed by interaction.

**/

(function () {
    'use strict';
    
    
    function UmbCheckboxController($timeout) {
        
        var vm = this;
        
        vm.callOnChange = function() {
            $timeout(function() {
                vm.onChange({model:vm.model, value:vm.value});
            }, 0);
        }
        
    }
    
    
    var component = {
        templateUrl: 'views/components/forms/umb-checkbox.html',
        controller: UmbCheckboxController,
        controllerAs: 'vm',
        bindings: {
            model: "=",
            inputId: "@",
            value: "@",
            name: "@",
            text: "@",
            localize: "@",
            serverValidationField: "@",
            disabled: "<",
            required: "<",
            onChange: "&"
        }
    };

    angular.module('umbraco.directives').component('umbCheckbox', component);

})();
