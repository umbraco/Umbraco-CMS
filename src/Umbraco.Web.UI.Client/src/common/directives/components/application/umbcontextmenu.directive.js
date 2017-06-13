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

            var eventBindings = [];

            //adds a handler to the context menu item click, we need to handle this differently
            //depending on what the menu item is supposed to do.
            scope.executeMenuItem = function (action) {
                navigationService.executeMenuAction(action, scope.currentNode, scope.currentSection);
            };

            function setFocus(actions) {
                if(actions && actions.length > 0) {
                    actions[0].hasFocus = true;
                }
            }

            function onInit() {
                setFocus(scope.menuActions);
            }

            onInit();

            eventBindings.push(scope.$watch('menuActions', function (newValue, oldValue) {
                if (newValue === oldValue) { return; }
                if (oldValue === undefined || newValue === undefined) { return; }
                setFocus(newValue);
            }));

            // clean up
            scope.$on('$destroy', function () {
                // unbind watchers
                for (var e in eventBindings) {
                    eventBindings[e]();
                }
            });

        }
    };
});
