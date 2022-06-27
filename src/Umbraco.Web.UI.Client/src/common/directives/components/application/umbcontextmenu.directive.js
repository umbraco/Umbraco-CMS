/**
* @ngdoc directive
* @name umbraco.directives.directive:umbContextMenu
* @restrict A
 *
 * @description
 * Handles the click events on the context menu
**/
angular.module("umbraco.directives")
.directive('umbContextMenu', function (navigationService, keyboardService) {
    return {
        scope: {
            menuDialogTitle: "@",
            currentSection: "@",
            currentNode: "=",
            menuActions: "="
        },
        restrict: 'E',
        replace: true,
        templateUrl: 'views/components/application/umb-contextmenu.html',
        link: function (scope, element, attrs, ctrl) {

            // Map action icons using legacy icon font or svg icons.
            Utilities.forEach(scope.menuActions, action => {
              action.icon = (action.useLegacyIcon ? 'icon-' : '') + action.icon;
            });
            
            //adds a handler to the context menu item click, we need to handle this differently
            //depending on what the menu item is supposed to do.
            scope.executeMenuItem = action => {
                navigationService.executeMenuAction(action, scope.currentNode, scope.currentSection);
            };
            
            scope.outSideClick = () => {
                navigationService.hideNavigation();
            };
            
            keyboardService.bind("esc", () =>  {
                navigationService.hideNavigation();
            });
            
            //ensure to unregister from all events!
            scope.$on('$destroy', () =>  {
                keyboardService.unbind("esc");
            });
        }
    };
});
