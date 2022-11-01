(function () {
    'use strict';

    function GridInlineBlockEditor($scope, $compile, $element) {

        const vm = this;

        var propertyEditorElement;

        vm.$onInit = function() {
            
            vm.property = $scope.block.content.variants[0].tabs[0]?.properties[0];

            if (vm.property) {
                vm.propertySlotName = "umbBlockGridProxy_" +vm.property.alias + "_" + String.CreateGuid();
                
                propertyEditorElement = $('<div slot="{{vm.propertySlotName}}"></div>');
                propertyEditorElement.html(`<umb-property-editor model="vm.property" preview="$scope.api.internal.readonly" ng-attr-readonly="{{$scope.api.internal.readonly || undefined}}"></umb-property-editor>`);

                const connectedCallback = () => {$compile(propertyEditorElement)($scope)};

                const event = new CustomEvent("UmbBlockGrid_AppendProperty", {composed: true, bubbles: true, detail: {'property': propertyEditorElement[0], 'slotName': vm.propertySlotName, 'connectedCallback':connectedCallback}});
                
                $element[0].dispatchEvent(event);
                
            }
        }

        vm.$onDestroy = function() {
            if (vm.property) {
                const event = new CustomEvent("UmbBlockGrid_RemoveProperty", {composed: true, bubbles: true, detail: {'property': propertyEditorElement, 'slotName': vm.propertySlotName}});
                $element[0].dispatchEvent(event);
            }
        }

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.GridInlineBlockEditor", GridInlineBlockEditor);

})();
