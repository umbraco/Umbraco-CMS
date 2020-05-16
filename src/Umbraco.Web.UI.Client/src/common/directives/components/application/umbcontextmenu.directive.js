angular.module("umbraco.directives")
.directive('umbContextMenu', function (navigationService, keyboardService, backdropService) {
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

            scope.outSideClick = function() {
                navigationService.hideNavigation();
                closeBackdrop();
            };

            keyboardService.bind("esc", function() {
                navigationService.hideNavigation();
                closeBackdrop();
            });

            //ensure to unregister from all events!
            scope.$on('$destroy', function () {
                keyboardService.unbind("esc");
            });

            function closeBackdrop() {
                var onTopClass = 'on-top-of-backdrop';
                var leftColumn = $('#leftcolumn');
                var isLeftColumnOnTop = leftColumn.hasClass(onTopClass);

                if(isLeftColumnOnTop){
                    backdropService.close();
                    leftColumn.removeClass(onTopClass);
                }
            }
        }
    };
});
