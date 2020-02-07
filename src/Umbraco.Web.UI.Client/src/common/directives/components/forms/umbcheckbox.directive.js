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
@param {string} inputId Set the <code>id</code> of the checkbox.
@param {string} value Set the value of the checkbox.
@param {string} name Set the name of the checkbox.
@param {string} text Set the text for the checkbox label.
@param {string} labelKey Set a dictinary/localization string for the checkbox label
@param {string} serverValidationField Set the <code>val-server-field</code> of the checkbox.
@param {boolean} disabled Set the checkbox to be disabled.
@param {boolean} required Set the checkbox to be required.
@param {callback} onChange Callback when the value of the checkbox change by interaction.
@param {string} cssClass Set a css class modifier

**/

(function () {
    'use strict';

    function UmbCheckboxController($timeout, localizationService) {

        var vm = this;

        vm.$onInit = onInit;
        vm.change = change;

        function onInit() {
            vm.inputId = vm.inputId || "umb-check_" + String.CreateGuid();

            // If a labelKey is passed let's update the returned text if it's does not contain an opening square bracket [
            if (vm.labelKey) {
                 localizationService.localize(vm.labelKey).then(function (data) {
                      if(data.indexOf('[') === -1){
                        vm.text = data;
                      }
                 });
            }
        }

        function change() {
            if (vm.onChange) {
                $timeout(function () {
                    vm.onChange({ model: vm.model, value: vm.value });
                }, 0);
            }
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
            labelKey: "@?",
            serverValidationField: "@",
            disabled: "<",
            required: "<",
            onChange: "&?",
            cssClass: "@?",
            iconClass: "@?"
        }
    };

    angular.module('umbraco.directives').component('umbCheckbox', component);

})();
