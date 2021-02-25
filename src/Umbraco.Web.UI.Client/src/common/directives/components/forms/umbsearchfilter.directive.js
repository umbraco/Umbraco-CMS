/**
@ngdoc directive
@name umbraco.directives.directive:umbSearchFilter
@restrict E
@scope

@description
<b>Added in Umbraco version 8.7.0</b> Use this directive to render an umbraco search filter.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-search-filter
            name="checkboxlist"
            value="{{key}}"
            model="true"
            text="{{text}}">
        </umb-search-filter>

    </div>
</pre>

@param {boolean} model Set to <code>true</code> or <code>false</code> to set the checkbox to checked or unchecked.
@param {string} inputId Set the <code>id</code> of the checkbox.
@param {string} text Set the text for the checkbox label.
@param {string} labelKey Set a dictinary/localization string for the checkbox label
@param {callback} onChange Callback when the value of the checkbox change by interaction.
@param {boolean} autoFocus Add autofocus to the input field
@param {boolean} preventSubmitOnEnter Set the enter prevent directive or not

**/

(function () {
    'use strict';

    function UmbSearchFilterController($timeout, localizationService) {

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
        templateUrl: 'views/components/forms/umb-search-filter.html',
        controller: UmbSearchFilterController,
        controllerAs: 'vm',
        transclude: true,
        bindings: {
            model: "=",
            inputId: "@",
            text: "@",
            labelKey: "@?",
            onChange: "&?",
            autoFocus: "<?",
            preventSubmitOnEnter: "<?",
            cssClass: "@?"
        }
    };

    angular.module('umbraco.directives').component('umbSearchFilter', component);

})();
