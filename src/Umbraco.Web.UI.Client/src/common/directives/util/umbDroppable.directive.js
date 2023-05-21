angular.module("umbraco.directives")
    .directive('umbDroppable', function ($timeout) {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                $timeout(() => {
                    const options = scope.$eval(attrs.umbDroppable)
                    element.droppable(options);
                });
            }
        }
    });
