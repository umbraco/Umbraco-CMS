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
@param {string} labelKey Set a dictionary/localization string for the checkbox label
@param {callback} onChange Callback when the value of the searchbox change.
@param {callback} onBack Callback when clicking back button.
@param {boolean} autoFocus Add autofocus to the input field
@param {boolean} preventSubmitOnEnter Set the enter prevent directive or not
@param {boolean} showBackButton Show back button on search

**/

(function () {
    'use strict';

    function UmbSearchFilterController($timeout, localizationService) {

        var vm = this;

        vm.$onInit = onInit;
        vm.change = change;
        vm.keyDown = keyDown;
        vm.blur = blur;
        vm.goBack = goBack;

        function onInit() {
            vm.inputId = vm.inputId || "umb-search-filter_" + String.CreateGuid();
            vm.autoFocus = Object.toBoolean(vm.autoFocus) === true;
            vm.preventSubmitOnEnter = Object.toBoolean(vm.preventSubmitOnEnter) === true;
            vm.showBackButton = Object.toBoolean(vm.showBackButton) === true;

            // If a labelKey is passed let's update the returned text if it's does not contain an opening square bracket [
            if (vm.labelKey) {
                 localizationService.localize(vm.labelKey).then(data => {
                      if (data.indexOf('[') === -1){
                         vm.text = data;
                      }
                 });
            }
        }

        function goBack() {
            if (vm.onBack) {
                vm.onBack();
            }
        }

        function change() {
            if (vm.onChange) {
                $timeout(function () {
                    vm.onChange({ model: vm.model, value: vm.value });
                }, 0);
            }
        }

        function blur() {
            if (vm.onBlur) {
                vm.onBlur();
            }
        }

        function keyDown(evt) {
            //13: enter
            switch (evt.keyCode) {
                case 13:
                    if (vm.onSearch) {
                        vm.onSearch();
                    }
                    break;
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
            onSearch: "&?",
            onBlur: "&?",
            onBack: "&?",
            autoFocus: "<?",
            preventSubmitOnEnter: "<?",
            showBackButton: "<?",
            cssClass: "@?"
        }
    };

    angular.module('umbraco.directives').component('umbSearchFilter', component);

})();
