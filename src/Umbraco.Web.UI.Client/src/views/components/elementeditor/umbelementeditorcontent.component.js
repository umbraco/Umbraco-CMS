(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbElementEditorContent', {
            templateUrl: 'views/components/elementeditor/umb-element-editor-content.component.html',
            controller: ElementEditorContentComponentController,
            controllerAs: 'vm',
            bindings: {
                model: '='
            }
        });

    function ElementEditorContentComponentController($scope) {

        // We need a controller for the component to work.
        var vm = this;

        vm.getScope = getScope;// used by property editors to get a scope that is the root of split view, content apps etc.
        function getScope() {
            return $scope;
        }

    }

})();
