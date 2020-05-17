/**
@ngdoc directive
@name umbraco.directives.directive:umbButtonEllipsis
@restrict E
@scope

@description
<b>Added in Umbraco version 8.7.0</b> Use this directive to render an umbraco ellipsis.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-button-ellipsis
            text="{{text}}"
            labelKey="{{labelKey}}"
            showLabel="{{showLabel}}"
            onClick="{{vm.open()}}"
            dataElement="{{element}}"
            >
        </umb-button-ellipsis>

    </div>
</pre>

@param {string} text Set the text for the checkbox label.
@param {string} labelKey Set a dictinary/localization string for the checkbox label
@param {callback} onClick Callback when the value of the checkbox change by interaction.
@param {string} cssClass Set a css class modifier
@param {boolean} showText Set to <code>true</code> to show the text. <code>false</code> by default
@param {string} element *TODO: Need to document this - Not sure why it's done like this*

**/

(function () {
    'use strict';

    function UmbButtonEllipsis($timeout, localizationService) {

        var vm = this;

        vm.$onInit = onInit;
        vm.click = click;

        function onInit() {
            // If a labelKey is passed let's update the returned text if it's does not contain an opening square bracket [
            if (vm.labelKey) {
                 localizationService.localize(vm.labelKey).then(function (data) {
                      if(data.indexOf('[') === -1){
                        vm.text = data;
                      }
                 });
            }
        }

        function click() {
            console.log('click click');
            if(vm.onClick) {
                vm.onClick(); // How to dynamically get the passed args?
            }

            // if (vm.onChange) {
            //     $timeout(function () {
            //         vm.onChange({ model: vm.model, value: vm.value });
            //     }, 0);
            // }
        }
    }

    var component = {
        templateUrl: 'views/components/buttons/umb-button-ellipsis.html',
        controller: UmbButtonEllipsis,
        controllerAs: 'vm',
        transclude: true,
        bindings: {
            text: "@",
            labelKey: "@?",
            onClick: "&",
            cssClass: "@?",
            showText: "<",
            element: "@?"
        }
    };

    angular.module('umbraco.directives').component('umbButtonEllipsis', component);

})();
