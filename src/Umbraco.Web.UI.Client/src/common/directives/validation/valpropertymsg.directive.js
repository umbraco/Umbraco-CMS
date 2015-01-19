/**
* @ngdoc directive
* @name umbraco.directives.directive:valPropertyMsg
* @restrict A
* @element textarea
* @requires formController
* @description This directive is used to control the display of the property level validation message.
* We will listen for server side validation changes
* and when an error is detected for this property we'll show the error message.
* In order for this directive to work, the valStatusChanged directive must be placed on the containing form.
**/
function valPropertyMsg(serverValidationManager) {

    return {
        scope: {
            property: "="
        },
        require: "^form",   //require that this directive is contained within an ngForm
        replace: true,      //replace the element with the template
        restrict: "E",      //restrict to element
        template: "<div ng-show=\"errorMsg != ''\" class='alert alert-error property-error' >{{errorMsg}}</div>",

        /**
            Our directive requries a reference to a form controller 
            which gets passed in to this parameter
         */
        link: function (scope, element, attrs, formCtrl) {

            var watcher = null;

            // Gets the error message to display
            function getErrorMsg() {
                //this can be null if no property was assigned
                if (scope.property) {
                    //first try to get the error msg from the server collection
                    var err = serverValidationManager.getPropertyError(scope.property.alias, "");
                    //if there's an error message use it
                    if (err && err.errorMsg) {
                        return err.errorMsg;
                    }
                    else {
                        return scope.property.propertyErrorMessage ? scope.property.propertyErrorMessage : "Property has errors";
                    }

                }
                return "Property has errors";
            }

            // We need to subscribe to any changes to our model (based on user input)
            // This is required because when we have a server error we actually invalidate 
            // the form which means it cannot be resubmitted. 
            // So once a field is changed that has a server error assigned to it
            // we need to re-validate it for the server side validator so the user can resubmit
            // the form. Of course normal client-side validators will continue to execute. 
            function startWatch() {
                //if there's not already a watch
                if (!watcher) {
                    watcher = scope.$watch("property.value", function (newValue, oldValue) {
                        
                        if (!newValue || angular.equals(newValue, oldValue)) {
                            return;
                        }

                        var errCount = 0;
                        for (var e in formCtrl.$error) {
                            if (angular.isArray(formCtrl.$error[e])) {
                                errCount++;
                            }
                        }

                        //we are explicitly checking for valServer errors here, since we shouldn't auto clear
                        // based on other errors. We'll also check if there's no other validation errors apart from valPropertyMsg, if valPropertyMsg
                        // is the only one, then we'll clear.

                        if ((errCount === 1 && angular.isArray(formCtrl.$error.valPropertyMsg)) || (formCtrl.$invalid && angular.isArray(formCtrl.$error.valServer))) {
                            scope.errorMsg = "";
                            formCtrl.$setValidity('valPropertyMsg', true);
                            stopWatch();
                        }
                    }, true);
                }
            }

            //clear the watch when the property validator is valid again
            function stopWatch() {
                if (watcher) {
                    watcher();
                    watcher = null;
                }
            }

            //if there's any remaining errors in the server validation service then we should show them.
            var showValidation = serverValidationManager.items.length > 0;
            var hasError = false;

            //create properties on our custom scope so we can use it in our template
            scope.errorMsg = "";

            //listen for form error changes
            scope.$on("valStatusChanged", function (evt, args) {
                if (args.form.$invalid) {

                    //first we need to check if the valPropertyMsg validity is invalid
                    if (formCtrl.$error.valPropertyMsg && formCtrl.$error.valPropertyMsg.length > 0) {
                        //since we already have an error we'll just return since this means we've already set the 
                        // hasError and errorMsg properties which occurs below in the serverValidationManager.subscribe
                        return;
                    }
                    else if (element.closest(".umb-control-group").find(".ng-invalid").length > 0) {
                        //check if it's one of the properties that is invalid in the current content property
                        hasError = true;
                        //update the validation message if we don't already have one assigned.
                        if (showValidation && scope.errorMsg === "") {
                            scope.errorMsg = getErrorMsg();
                        }
                    }
                    else {
                        hasError = false;
                        scope.errorMsg = "";
                    }
                }
                else {
                    hasError = false;
                    scope.errorMsg = "";
                }
            }, true);

            //listen for the forms saving event
            scope.$on("formSubmitting", function (ev, args) {
                showValidation = true;
                if (hasError && scope.errorMsg === "") {
                    scope.errorMsg = getErrorMsg();
                }
                else if (!hasError) {
                    scope.errorMsg = "";
                    stopWatch();
                }
            });

            //listen for the forms saved event
            scope.$on("formSubmitted", function (ev, args) {
                showValidation = false;
                scope.errorMsg = "";
                formCtrl.$setValidity('valPropertyMsg', true);
                stopWatch();
            });

            //listen for server validation changes
            // NOTE: we pass in "" in order to listen for all validation changes to the content property, not for
            // validation changes to fields in the property this is because some server side validators may not
            // return the field name for which the error belongs too, just the property for which it belongs.
            // It's important to note that we need to subscribe to server validation changes here because we always must
            // indicate that a content property is invalid at the property level since developers may not actually implement
            // the correct field validation in their property editors.

            if (scope.property) { //this can be null if no property was assigned
                serverValidationManager.subscribe(scope.property.alias, "", function (isValid, propertyErrors, allErrors) {
                    hasError = !isValid;
                    if (hasError) {
                        //set the error message to the server message
                        scope.errorMsg = propertyErrors[0].errorMsg;
                        //flag that the current validator is invalid
                        formCtrl.$setValidity('valPropertyMsg', false);
                        startWatch();
                    }
                    else {
                        scope.errorMsg = "";
                        //flag that the current validator is valid
                        formCtrl.$setValidity('valPropertyMsg', true);
                        stopWatch();
                    }
                });

                //when the element is disposed we need to unsubscribe!
                // NOTE: this is very important otherwise when this controller re-binds the previous subscriptsion will remain
                // but they are a different callback instance than the above.
                element.bind('$destroy', function () {
                    stopWatch();
                    serverValidationManager.unsubscribe(scope.property.alias, "");
                });
            }
        }
    };
}
angular.module('umbraco.directives.validation').directive("valPropertyMsg", valPropertyMsg);