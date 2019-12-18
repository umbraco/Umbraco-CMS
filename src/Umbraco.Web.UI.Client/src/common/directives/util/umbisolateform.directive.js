angular.module("umbraco.directives")
    .directive('umbIsolateForm', function () {
        return {
            restrict: 'A',
            require: ['form', '^form'],
            link: function (scope, element, attrs, forms) {
                forms[1].$removeControl(forms[0]);
            }
        }
    });
