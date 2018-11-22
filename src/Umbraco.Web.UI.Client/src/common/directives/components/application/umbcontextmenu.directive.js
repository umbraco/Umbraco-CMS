angular.module("umbraco.directives")
.directive('umbContextMenu', function (navigationService) {
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

            //adds a handler to the context menu item click, we need to handle this differently
            //depending on what the menu item is supposed to do.
            scope.executeMenuItem = function (action) {
                navigationService.executeMenuAction(action, scope.currentNode, scope.currentSection);
            };
        }
    };
});
