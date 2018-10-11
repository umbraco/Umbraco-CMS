(function () {
    "use strict";

    function showValidationOnSubmit(serverValidationManager) {
        return {
            require: ["ngMessages", "^^?valFormManager"],
            restrict: "A",
            scope: {
                form: "=?"
            },
            link: function (scope, element, attr, ctrl) {

                var formMgr = ctrl.length > 1 ? ctrl[1] : null;

                //We can either get the form submitted status by the parent directive valFormManager
                //or we can check upwards in the DOM for the css class... lets try both :)
                //The initial hidden state can't always be hidden because when we switch variants in the content editor we cannot
                //reset the status.
                var submitted = element.closest(".show-validation").length > 0 || (formMgr && formMgr.showValidation);
                if (!submitted) {
                    element.hide();
                }

                var unsubscribe = [];

                unsubscribe.push(scope.$on("formSubmitting", function (ev, args) {
                    element.show();
                }));

                unsubscribe.push(scope.$on("formSubmitted", function (ev, args) {
                    element.hide();
                }));

                //no isolate scope to listen to element destroy
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
