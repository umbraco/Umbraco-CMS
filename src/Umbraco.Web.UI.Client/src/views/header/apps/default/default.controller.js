(function () {
    "use strict";

    function HeaderAppDefaultController($scope, $injector, focusService) {
        $scope.rememberFocus = focusService.rememberFocus;

        $scope.executeAction = function (action) {
            if (!action) {
                throw "action cannot be null";
            }

            var headerAction = action.split('.');
            if (headerAction.length !== 2 || action.includes('javascript:')) {
                eval(action.replace("javascript:", ""));
            } else {
                var headerActionService = $injector.get(headerAction[0]);
                if (!headerActionService) {
                    throw "The angular service " + headerAction[0] + " could not be found";
                }

                var method = headerActionService[headerAction[1]];

                if (!method) {
                    throw "The method " + headerAction[1] + " on the angular service " + headerAction[0] + " could not be found";
                }

                method.apply(this);
            }
        }
    }

    angular.module("umbraco").controller("Umbraco.Editors.Headers.Apps.DefaultController", HeaderAppDefaultController);
})();
