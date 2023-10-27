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
            action="{{vm.open()}}"
            >
        </umb-button-ellipsis>

    </div>
</pre>

@param {callback} action Callback when the value of the checkbox changes through interaction.
@param {string} text Set the text for the checkbox label.
@param {string=} labelKey Set a dictionary/localization string for the checkbox label
@param {string=} cssClass Set a css class modifier
@param {string=} color Set a hex code e.g. <code>#f5c1bc</code>. <code>#000000</code> by default
@param {boolean=} showText Set to <code>true</code> to show the text. <code>false</code> by default
@param {string=} element Highlights a DOM-element (HTML selector) e.g. "my-div-name"
@param {string=} state Set the initial state of the component. To have it hidden use <code>hidden</code>
@param {string=} mode Set the mode, which decides how to style the component. "small" and "tab" are currently supported
**/

(function () {
    'use strict';

    function UmbButtonEllipsis($timeout, localizationService) {

        var vm = this;

        vm.$onInit = onInit;
        vm.clickButton = clickButton;

        function onInit() {
            setText();
            setColor();
        }

        function clickButton(event) {
            if(vm.action) {
                vm.action({$event: event});
            }
        }

        function setText() {
            if (vm.labelKey) {
                localizationService.localize(vm.labelKey).then(function (data) {
                    // If a labelKey is passed let's update the returned text if it's does not contain an opening square bracket [
                     if(data.indexOf('[') === -1){
                       vm.text = data;
                     }
                });
           }
        }

        function setColor() {
            vm.color = vm.color ? vm.color : '#000000';
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
            color: "@?",
            showText: "<?",
            element: "@?",
            state: "@?",
            mode: "@?"
        }
    };

    angular.module('umbraco.directives').component('umbButtonEllipsis', component);

})();
