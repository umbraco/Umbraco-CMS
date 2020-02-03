(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbElementEditor', {
            templateUrl: 'views/common/infiniteeditors/elementeditor/elementeditor.component.html',
            controller: ElementEditorComponentController,
            controllerAs: 'vm',
            bindings: {
                content: '='
            }
        });

    function ElementEditorComponentController() {

        const vm = this;

        // TODO: we might not need this..

    }

})();
