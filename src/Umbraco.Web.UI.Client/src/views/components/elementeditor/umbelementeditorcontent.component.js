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

    function ElementEditorContentComponentController($scope, $filter, contentEditingHelper, contentTypeHelper) {

        // We need a controller for the component to work.
        var vm = this;

        vm.tabs = [];
        vm.activeTabAlias = null;

        vm.getScope = getScope; // used by property editors to get a scope that is the root of split view, content apps etc.
        vm.setActiveTab = setActiveTab;

        $scope.$watchCollection('vm.model.variants[0].tabs', (newValue) => {

            contentTypeHelper.defineParentAliasOnGroups(newValue);
            contentTypeHelper.relocateDisorientedGroups(newValue);

            vm.tabs = $filter("filter")(newValue, (tab) => {
                return tab.type === contentTypeHelper.TYPE_TAB;
            });

            if (vm.tabs.length > 0) {
                // if we have tabs and some groups that doesn't belong to a tab we need to render those on an "Other" tab.
                contentEditingHelper.registerGenericTab(newValue);

                setActiveTab(vm.tabs[0]);
            }
        });

        $scope.$watchCollection('vm.model.variants', (newValue) => {
            if (newValue && newValue.length > 0) {
                vm.allowUpdate = newValue[0].allowedActions.includes('A');
            }
        });

        function getScope() {
            return $scope;
        }

        function setActiveTab (tab) {
            vm.activeTabAlias = tab.alias;
            vm.tabs.forEach(tab => tab.active = false);
            tab.active = true;
        }
    }

})();
