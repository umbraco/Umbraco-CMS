(function () {
    'use strict';

    // TODO: Add docs - this component is used to render a content item based on an Element Type as a nested editor

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

    function ElementEditorContentComponentController($scope, $filter) {

        // We need a controller for the component to work.
        var vm = this;

        vm.tabs = [];
        vm.activeTabKey = '';

        vm.getScope = getScope; // used by property editors to get a scope that is the root of split view, content apps etc.
        vm.setActiveTab = setActiveTab;
        
        $scope.$watchCollection('vm.model.variants[0].tabs', () => {
            vm.tabs = $filter("filter")(vm.model.variants[0].tabs, (tab) => {
                return tab.type === 1;
            });

            if (vm.tabs.length > 0 && !vm.activeTabKey) {
                vm.activeTabKey = vm.tabs[0].key;
            }

            // for validation to work for each tab we need to associate a group with a tab.
            vm.model.variants[0].tabs.forEach(group => {
                let validationAlias = group.alias;

                if (group.parentKey) {
                    const parentGroup = vm.model.variants[0].tabs.find(tab => tab.key === group.parentKey);
                    validationAlias = parentGroup.alias;
                }

                group.validationAlias = validationAlias;
            });
        });

        function getScope() {
            return $scope;
        }

        function setActiveTab (tab) {
            vm.activeTabKey = tab.key;
            vm.tabs.forEach(tab => tab.active = false);
            tab.active = true;
        }
    }

})();
