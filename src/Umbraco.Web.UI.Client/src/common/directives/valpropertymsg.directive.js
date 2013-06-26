/**
    * @ngdoc directive 
    * @name umbraco.directives:valPropertyMsg
    * @restrict A
    * @description This directive is used to control the display of the property level validation message.
    *    We will listen for server side validation changes
    *    and when an error is detected for this property we'll show the error message and then we need 
    *    to emit the valBubble event so that any parent listening can update it's UI (like the validation summary)
    **/
function valPropertyMsg(serverValidationService) {
    return {        
        require: "^form",   //require that this directive is contained within an ngForm
        replace: true,      //replace the element with the template
        restrict: "E",      //restrict to element
        template: "<div ng-show=\"errorMsg != ''\" class='alert alert-error' >{{errorMsg}}</div>",
        
        /**
         * @ngdoc function
         * @name link
         * @methodOf valPropertyMsg
         * @function
         *
         * @description
         * The linking function for the directive
         *
         * @param formCtrl {FormController} Our directive requries a reference to a form controller which gets passed in to this parameter
         */
        link: function (scope, element, attrs, formCtrl) {
            
            if (!attrs.property) {
                throw "the valPropertyMsg requires an attribute 'property' set which equals the current content property object";
            }
            var currentProperty = scope.$eval(attrs.property);

            //flags for use in the below closures
            var showValidation = false;
            var hasError = false;

            //create properties on our custom scope so we can use it in our template
            scope.errorMsg = "";

            //listen for form validation
            scope.$watch(formCtrl.$name + ".$valid", function (isValid, oldValue) {
                if (isValid === undefined) {
                    return;
                }

                if (!isValid) {
                    //check if it's one of the properties that is invalid in the current content property
                    if (element.closest(".umb-control-group").find(".ng-invalid").length > 0) {
                        hasError = true;
                        if (showValidation && scope.errorMsg === "") {
                            //update the validation message
                            scope.errorMsg = serverValidationService.getError(currentProperty, "");
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
            });

            //listen for the forms saving event
            scope.$on("saving", function (ev, args) {
                showValidation = true;
                if (hasError && scope.errorMsg === "") {
                    scope.errorMsg = serverValidationService.getError(currentProperty, "");
                }
                else if (!hasError) {
                    scope.errorMsg = "";
                }
            });

            //listen for the forms saved event
            scope.$on("saved", function (ev, args) {
                showValidation = false;
                scope.errorMsg = "";
            });

            //listen for server validation changes
            // NOTE: we pass in "" in order to listen for all validation changes to the content property, not for
            // validation changes to fields in the property this is because some server side validators may not
            // return the field name for which the error belongs too, just the property for which it belongs.
            serverValidationService.subscribe(currentProperty, "", function (isValid, propertyErrors, allErrors) {
                hasError = !isValid;
                if (hasError) {
                    //set the error message to the server message
                    scope.errorMsg = propertyErrors[0].errorMsg;
                    //now that we've used the server validation message, we need to remove it from the 
                    //error collection... it is a 'one-time' usage so that when the field is invalidated 
                    //again, the message we display is the client side message.
                    //NOTE: 'this' in the subscribe callback context is the validation manager object.
                    this.removeError(scope.model);
                    //emit an event upwards 
                    scope.$emit("valBubble", {
                        isValid: false,         // it is INVALID
                        element: element,       // the element that the validation applies to
                        scope: scope,           // the scope
                        formCtrl: formCtrl      // the current form controller
                    });
                }
                else {
                    scope.errorMsg = "";
                    //emit an event upwards 
                    scope.$emit("valBubble", {
                        isValid: true,          // it is VALID
                        element: element,       // the element that the validation applies to
                        scope: scope,           // the scope
                        formCtrl: formCtrl      // the current form controller
                    });
                }
            });
            
            //when the element is disposed we need to unsubscribe!
            element.bind('$destroy', function () {
                serverValidationService.unsubscribe(currentProperty, "");
            });
        }
    };
}
angular.module('umbraco.directives').directive("valPropertyMsg", valPropertyMsg);