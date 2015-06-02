angular.module("umbraco.directives")
    .directive('umbSubViews', function () {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/subviews/umb-sub-views.html',
            link: function (scope, element, attrs, ctrl) {

                scope.tools = [];

                scope.toggleView = function(selectedView) {

                    // set all other views to inactive
                    for (var index = 0; index < scope.page.subViews.length; index++) {
                        var view = scope.page.subViews[index];
                        view.isActive = false;
                    }

                    // activate view
                    selectedView.isActive = true;

                    // set tool bar
                    setToolBar(selectedView);
                };

                function setToolBar(selectedView) {
                    scope.tools = selectedView.tools;
                }

                function init() {
                    // set first element to active
                    scope.page.subViews[0].isActive = true;

                    // set tools from first view
                    scope.tools = scope.page.subViews[0].tools;
                }

                scope.$watch('page.subViews', function(newValue, oldValue) {
                    if (newValue) {
                        // wait for sub views to be loaded - then init
                        init();
                    }
                });

            }
        };
    });