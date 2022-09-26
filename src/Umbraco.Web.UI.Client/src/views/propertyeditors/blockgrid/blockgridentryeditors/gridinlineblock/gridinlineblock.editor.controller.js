(function () {
    'use strict';

    function GridInlineBlockEditor($scope, $element) {

        const vm = this;

        vm.$onInit = function() {
           const host =  $element[0].getRootNode();

           console.log(document.styleSheets)

            for (const stylesheet of document.styleSheets) {

                console.log(stylesheet);
                const styleEl = document.createElement('link');
                styleEl.setAttribute('rel', 'stylesheet');
                styleEl.setAttribute('type', stylesheet.type);
                styleEl.setAttribute('href', stylesheet.href);

                host.appendChild(styleEl);
            }
        }

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.GridInlineBlockEditor", GridInlineBlockEditor);

})();
