angular.module("umbraco.directives")
    .directive('umbDroppable', function () {
        return {
            restrict: 'A',            
            link: function (scope, element, attrs) {
                const options = scope.$eval(attrs.umbDroppable)
                element.droppable(options);
            }
        }
    });
