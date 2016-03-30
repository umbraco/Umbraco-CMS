/**
* @ngdoc directive
* @name umbraco.directives.directive:umbOptionsMenu
* @deprecated
* We plan to remove this directive in the next major version of umbraco (8.0). The directive is not recommended to use.
* @function
* @element ANY
* @restrict E
**/

angular.module("umbraco.directives")
.directive('umbOptionsMenu', function ($injector, treeService, navigationService, umbModelMapper, appState) {
    return {
        scope: {
            currentSection: "@",
            currentNode: "="
        },
        restrict: 'E',
        replace: true,
        templateUrl: 'views/directives/_obsolete/umb-optionsmenu.html',
        link: function (scope, element, attrs, ctrl) {

            //adds a handler to the context menu item click, we need to handle this differently
            //depending on what the menu item is supposed to do.
            scope.executeMenuItem = function (action) {
                navigationService.executeMenuAction(action, scope.currentNode, scope.currentSection);
            };

            //callback method to go and get the options async
            scope.getOptions = function () {

                if (!scope.currentNode) {
                    return;
                }

                //when the options item is selected, we need to set the current menu item in appState (since this is synonymous with a menu)
                appState.setMenuState("currentNode", scope.currentNode);

                if (!scope.actions) {
                    treeService.getMenu({ treeNode: scope.currentNode })
                        .then(function (data) {
                            scope.actions = data.menuItems;
                        });
                }
            };

        }
    };
});
