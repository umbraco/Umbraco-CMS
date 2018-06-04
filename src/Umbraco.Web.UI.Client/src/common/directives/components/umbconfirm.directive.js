/**
 * @ngdoc directive
 * @name umbraco.directives.directive:umbConfirm
 * @function
 * @description
 * A confirmation dialog
 *
 * @restrict E
 */
function confirmDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/components/umb-confirm.html',
        scope: {
            onConfirm: '=',
            onCancel: '=',
            caption: '@'
        },
        link: function (scope, element, attr, ctrl) {
            scope.showCancel = false;
            scope.showConfirm = false;

            if (scope.onConfirm) {
                scope.showConfirm = true;
            }

            if (scope.onCancel) {
                scope.showCancel = true;
            }
        }
    };
}
angular.module('umbraco.directives').directive("umbConfirm", confirmDirective);
