angular.module("umbraco.directives")
    .directive('isolateForm', function ($window, $timeout, windowResizeListener) {
        return {
            restrict: 'A',
            require: ['form', '^form'],
            link: function(scope, element, attrs, forms) {
                forms[1].$removeControl(forms[0]);
            }
        }
    });
