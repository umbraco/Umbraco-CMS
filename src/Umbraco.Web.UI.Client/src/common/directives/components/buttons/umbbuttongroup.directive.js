angular.module("umbraco.directives")
    .directive('umbButtonGroup', function () {
        return {
            scope: {
                actions: "=",
                model: "="
            },
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/buttons/umb-button-group.html',
            link: function (scope, element, attrs, ctrl) {

                scope.primaryAction = {};
                scope.secondaryActions = [];

                function generateActions() {

                    for (var index = 0; index < scope.actions.length; index++) {

                        var action = scope.actions[index];

                        if( index === 0 ) {
                            scope.primaryAction = action;
                        } else {
                            scope.secondaryActions.push(action);
                        }
                    }

                }

                scope.$watch('actions', function(newValue, oldValue) {
                    if (newValue) {
                        generateActions();
                    }
                });

            }
        };
    });