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
            action="{{vm.open()}}"
            element="{{element}}"
            >
        </umb-button-ellipsis>

    </div>
</pre>

@param {string} text Set the text for the checkbox label.
@param {string} labelKey Set a dictinary/localization string for the checkbox label
@param {callback} action Callback when the value of the checkbox change by interaction.
@param {string} cssClass Set a css class modifier
@param {boolean} showText Set to <code>true</code> to show the text. <code>false</code> by default
@param {string} element *TODO: Need to document this - Not sure why it's done like this*
@param {string} state Set the initial state of the component. To have it hidden use <code>hidden</code>
**/

(function () {
    'use strict';

    function UmbButtonEllipsis($timeout, localizationService) {

        var vm = this;

        vm.$onInit = onInit;
        vm.clickButton = clickButton;

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

        function clickButton(event) {
            if(vm.action) {
                vm.action({$event: event});
            }
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
            action: "&",
            cssClass: "@?",
            showText: "<?",
            element: "@?",
            state: "@?"
        }
    };

    angular.module('umbraco.directives').component('umbButtonEllipsis', component);

})();
