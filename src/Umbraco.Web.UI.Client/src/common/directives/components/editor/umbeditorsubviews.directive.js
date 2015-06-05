angular.module("umbraco.directives")
    .directive('umbEditorSubViews', function () {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-sub-views.html',
            link: function (scope, element, attrs, ctrl) {

                scope.tools = [];
                scope.activeView = {};

                // set toolbar from selected navigation item
                function setToolBar(items) {

                    scope.tools = [];

                    for (var index = 0; index < items.length; index++) {
                        var item = items[index];

                        if(item.active && item.tools) {
                            scope.tools = item.tools;
                        }

                        if(item.active && item.view) {
                            scope.activeView = item;
                        }
                    }
                }

                // watch for navigation changes
                scope.$watch('page.navigation', function(newValue, oldValue) {
                    if (newValue) {

                        setToolBar(newValue);
                    }
                },true);

            }
        };
    });