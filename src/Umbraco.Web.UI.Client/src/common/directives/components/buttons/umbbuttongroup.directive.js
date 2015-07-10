angular.module("umbraco.directives")
    .directive('umbButtonGroup', function () {
        return {
            scope: {
                actions: "=",
                model: "="
            },
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/buttons/umb-button-group.html'
        };
    });
