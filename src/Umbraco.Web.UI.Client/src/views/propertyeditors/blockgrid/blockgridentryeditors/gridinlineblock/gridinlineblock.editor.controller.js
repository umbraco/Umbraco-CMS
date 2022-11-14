(function () {
    'use strict';

    function GridInlineBlockEditor($scope, $compile, $element) {

        const vm = this;

        var propertyEditorElement;

        vm.$onInit = function() {
            
            vm.property = $scope.block.content.variants[0].tabs[0]?.properties[0];

            if (vm.property) {
                vm.propertySlotName = "umbBlockGridProxy_" + vm.property.alias + "_" + String.CreateGuid();
                
                propertyEditorElement = $('<div slot="{{vm.propertySlotName}}"></div>');
                propertyEditorElement.html(
                    `
                    <umb-property
                        data-element="grid-block-property-{{vm.property.alias}}"
                        property="vm.property"
                        node="$scope.block.content"
                        hide-label="true">

                        <umb-property-editor 
                            model="vm.property" 
                            preview="$scope.api.internal.readonly" 
                            ng-attr-readonly="{{$scope.api.internal.readonly || undefined}}">
                        </umb-property-editor>

                    </umb-property>
                    `
                );
                
                $element[0].addEventListener('umb-rte-focus', onRteFocus);
                $element[0].addEventListener('umb-rte-blur', onRteBlur);

                const connectedCallback = () => {
                    
                    $compile(propertyEditorElement)($scope)
                };
                
                const event = new CustomEvent("UmbBlockGrid_AppendProperty", {composed: true, bubbles: true, detail: {'property': propertyEditorElement[0], 'contentUdi': $scope.block.layout.contentUdi, 'slotName': vm.propertySlotName, 'connectedCallback':connectedCallback}});
                
                $element[0].dispatchEvent(event);
                
            }
        }

        function onRteFocus() {
            $element[0].classList.add('umb-block-grid--force-focus');
        }
        function onRteBlur() {
            $element[0].classList.remove('umb-block-grid--force-focus');
        }

        vm.$onDestroy = function() {
            if (vm.property) {
                const event = new CustomEvent("UmbBlockGrid_RemoveProperty", {composed: true, bubbles: true, detail: {'slotName': vm.propertySlotName}});
                $element[0].dispatchEvent(event);
            }

            $element[0].removeEventListener('umb-rte-focus', onRteFocus);
            $element[0].removeEventListener('umb-rte-blur', onRteBlur);
            propertyEditorElement = null;
        }

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.GridInlineBlockEditor", GridInlineBlockEditor);

})();
