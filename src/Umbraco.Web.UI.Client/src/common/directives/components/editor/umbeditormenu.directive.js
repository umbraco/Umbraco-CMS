(function () {
    'use strict';

    function EditorMenuDirective($injector, treeService, navigationService, umbModelMapper, appState) {

        function link(scope, el, attr, ctrl) {

            scope.dropdown = {
                isOpen: false
            };

            function onInit() {

                getOptions();

            }

            //adds a handler to the context menu item click, we need to handle this differently
            //depending on what the menu item is supposed to do.
            scope.executeMenuItem = function (action) {
                navigationService.executeMenuAction(action, scope.currentNode, scope.currentSection);
                scope.dropdown.isOpen = false;
            };

            //callback method to go and get the options async
            function getOptions() {

                if (!scope.currentNode) {
                    return;
                }

                //when the options item is selected, we need to set the current menu item in appState (since this is synonymous with a menu)
                // Niels: No i think we are wrong, we should not set the currentNode, cause it represents the currentNode of interaction.
                //appState.setMenuState("currentNode", scope.currentNode);
                
                if (!scope.actions) {
                    treeService.getMenu({ treeNode: scope.currentNode })
                        .then(function (data) {
                            scope.actions = data.menuItems;
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
                currentSection: "@"
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbEditorMenu', EditorMenuDirective);

})();
