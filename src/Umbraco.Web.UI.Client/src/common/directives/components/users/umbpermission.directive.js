(function () {
    'use strict';

    function PermissionDirective() {

        function link(scope, el, attr, ctrl) {

            scope.change = function() {
                scope.selected = !scope.selected;
                if(scope.onChange) {
                    scope.onChange({'selected': scope.selected});
                }
            };

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/users/umb-permission.html',
            scope: {
                name: "=",
                description: "=?",
                selected: "=",
                onChange: "&"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbPermission', PermissionDirective);

})();