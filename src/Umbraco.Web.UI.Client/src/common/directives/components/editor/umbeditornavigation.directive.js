angular.module("umbraco.directives")
    .directive('umbEditorNavigation', function () {
        return {
            scope: {
                navigation: "="
            },
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-navigation.html',
            link: function (scope, element, attrs, ctrl) {

                scope.clickNavigationItem = function(selectedItem) {
                    setItemToActive(selectedItem);
                    runItemAction(selectedItem);
                };

                function runItemAction(selectedItem) {
                    if(selectedItem.action) {
                        selectedItem.action(selectedItem);
                    }
                }

                function setItemToActive(selectedItem) {
                    // set all other views to inactive
                    if(selectedItem.view) {

                        for (var index = 0; index < scope.navigation.length; index++) {
                            var item = scope.navigation[index];
                            item.active = false;
                        }

                        // set view to active
                        selectedItem.active = true;

                    }
                }

            }
        };
    });