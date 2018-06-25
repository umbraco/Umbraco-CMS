(function () {
    "use strict";

    function showValidationOnSubmit(serverValidationManager) {
        return {
            require: "ngMessages",
            restrict: "A",

            link: function (scope, element, attr, ctrl) {

                element.hide();

                var unsubscribe = [];

                unsubscribe.push(scope.$on("formSubmitting", function (ev, args) {
                    element.show();
                }));

                unsubscribe.push(scope.$on("formSubmitted", function (ev, args) {
                    element.hide();
                }));

                element.bind('$destroy', function () {
                    for (var u in unsubscribe) {
                        unsubscribe[u]();
                    }
                });

            }
        };
    }

    angular.module('umbraco.directives.validation').directive("showValidationOnSubmit", showValidationOnSubmit);

})();
