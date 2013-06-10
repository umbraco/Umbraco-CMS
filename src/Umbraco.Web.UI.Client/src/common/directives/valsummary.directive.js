/**
    * @ngdoc directive 
    * @name umbraco.directive:valSummary
    * @restrict E
    * @description This directive will display a validation summary for the current form based on the 
                    content properties of the current content item.
    **/
function valSummary() {
    return {
        scope: true,   // create a new scope for this directive
        replace: true,   // replace the html element with the template
        restrict: "E",    // restrict to an element
        template: '<ul class="validation-summary"><li ng-repeat="model in validationSummary">{{model}}</li></ul>',
        link: function (scope, element, attr, ctrl) {

            //create properties on our custom scope so we can use it in our template
            scope.validationSummary = [];

            //create a flag for us to be able to reference in the below closures for watching.
            var showValidation = false;

            //add a watch to update our waitingOnValidation flag for use in the below closures
            scope.$watch("$parent.ui.waitingOnValidation", function (isWaiting, oldValue) {
                showValidation = isWaiting;
                if (scope.validationSummary.length > 0 && showValidation) {
                    element.show();
                }
                else {
                    element.hide();
                }
            });

            //if we are to show field property based errors.
            //this requires listening for bubbled events from valBubble directive.

            scope.$parent.$on("valBubble", function (evt, args) {
                var msg = "The value assigned for the property " + args.scope.model.label + " is invalid";
                var exists = _.contains(scope.validationSummary, msg);

                //we need to check if the entire property is valid, even though the args says this field is valid there
                // may be multiple values attached to a content property. The easiest way to do this is check the DOM
                // just like we are doing for the property level validation message.
                var propertyHasErrors = args.element.closest(".content-property").find(".ng-invalid").length > 0;

                if (args.isValid && exists && !propertyHasErrors) {
                    //it is valid but we have a val msg for it so we'll need to remove the message
                    scope.validationSummary = _.reject(scope.validationSummary, function (item) {
                        return item === msg;
                    });
                }
                else if (!args.isValid && !exists) {
                    //it is invalid and we don't have a msg for it already
                    scope.validationSummary.push(msg);
                }

                //show the summary if there are errors and the form has been submitted
                if (showValidation && scope.validationSummary.length > 0) {
                    element.show();
                }
            });
            //listen for form invalidation so we know when to hide it
            scope.$watch("contentForm.$error", function (errors) {
                //check if there is an error and hide the summary if not
                var hasError = _.find(errors, function (err) {
                    return (err.length && err.length > 0);
                });
                if (!hasError) {
                    element.hide();
                }
            }, true);
        }
    };
}
angular.module('umbraco.directives').directive("valSummary", valSummary);