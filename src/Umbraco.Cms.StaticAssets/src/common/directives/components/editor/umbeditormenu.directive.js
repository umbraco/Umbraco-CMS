(function () {
    'use strict';

    function EditorMenuDirective(treeService, navigationService, appState) {

        function link(scope, el, attr, ctrl) {

            scope.dropdown = {
                isOpen: false
            };

            function onInit() {

                getOptions();

            }

            //adds a handler to the context menu item click, we need to handle this differently
            //depending on what the menu item is supposed to do.
            scope.executeMenuItem = action => {
                //the action is called as it would be by the tree. to ensure that the action targets the correct node, 
                //we need to set the current node in appState before calling the action. otherwise we break all actions
                //that use the current node (and that's pretty much all of them)
                appState.setMenuState("currentNode", scope.currentNode);                
                navigationService.executeMenuAction(action, scope.currentNode, scope.currentSection);
                scope.dropdown.isOpen = false;
            };

            //callback method to go and get the options async
            function getOptions() {

                if (!scope.currentNode) {
                    return;
                }

                if (!scope.actions) {
                    treeService.getMenu({ treeNode: scope.currentNode }).then(data => {
                        scope.actions = data.menuItems;

                        // Map action icons using legacy icon font or svg icons.
                        Utilities.forEach(scope.actions, action => {
                          action.icon = (action.useLegacyIcon ? 'icon-' : '') + action.icon;
                        });
                    });
                }
            };

            onInit();

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-menu.html',
            link: link,
            scope: {
                currentNode: "=",
                currentSection: "@",
                isDisabled: "<?"
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbEditorMenu', EditorMenuDirective);

})();
