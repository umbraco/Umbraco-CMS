(function () {
    'use strict';

    function GridInlineBlockEditor($scope, $element) {

        const vm = this;


        vm.$onInit = function() {
            
            vm.property = $scope.block.content.variants[0].tabs[0]?.properties[0];

           const host =  $element[0].getRootNode();

            for (const stylesheet of document.styleSheets) {

                if(stylesheet.href !== null && stylesheet.type === "text/css") {

                    const styleEl = document.createElement('link');
                    styleEl.setAttribute('rel', 'stylesheet');
                    styleEl.setAttribute('type', stylesheet.type);
                    styleEl.setAttribute('href', stylesheet.href);

                    host.appendChild(styleEl);
                }
            }
        }

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.GridInlineBlockEditor", GridInlineBlockEditor);

})();
