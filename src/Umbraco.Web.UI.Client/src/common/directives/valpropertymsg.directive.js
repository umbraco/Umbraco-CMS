/**
* @ngdoc object 
* @name umbraco.directives:valPropertyMsg
* @restrict A
* @element textarea
* @requires formController
* @description This directive is used to control the display of the property level validation message.
*    We will listen for server side validation changes
*    and when an error is detected for this property we'll show the error message
**/
function valPropertyMsg(serverValidationService) {
    return {
        scope: {
            currentProperty: "=property"  
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

            //assign the form control to our isolated scope so we can watch it's values
            scope.formCtrl = formCtrl;

            //flags for use in the below closures
            var showValidation = false;
            var hasError = false;

            //create properties on our custom scope so we can use it in our template
            scope.errorMsg = "";

            //listen for error changes
            scope.$watch("formCtrl.$error", function () {
                if (formCtrl.$valid === undefined) {
                    return;
                }

                if (!formCtrl.$valid) {
                    
                    //first we need to check if the valPropertyMsg validity is invalid
                    if (formCtrl.$error.valPropertyMsg && formCtrl.$error.valPropertyMsg.length > 0) {
                        //since we already have an error we'll just return since this means we've already set the 
                        // hasError and errorMsg properties which occurs below in the serverValidationService.subscribe
                        return;
                    }                    
                    else if (element.closest(".umb-control-group").find(".ng-invalid").length > 0) {
                        //check if it's one of the properties that is invalid in the current content property
                        hasError = true;
                        //update the validation message if we don't already have one assigned.
                        if (showValidation && scope.errorMsg === "") {                            
                            scope.errorMsg = serverValidationService.getError(scope.currentProperty, "");
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
            scope.$on("saving", function (ev, args) {
                showValidation = true;
                if (hasError && scope.errorMsg === "") {
                    scope.errorMsg = serverValidationService.getError(scope.currentProperty, "");
                }
                else if (!hasError) {
                    scope.errorMsg = "";
                }
            });

            //listen for the forms saved event
            scope.$on("saved", function (ev, args) {
                showValidation = false;
                scope.errorMsg = "";
                formCtrl.$setValidity('valPropertyMsg', true);                
            });

            //We need to subscribe to any changes to our model (based on user input)
            // This is required because when we have a server error we actually invalidate 
            // the form which means it cannot be resubmitted. 
            // So once a field is changed that has a server error assigned to it
            // we need to re-validate it for the server side validator so the user can resubmit
            // the form. Of course normal client-side validators will continue to execute.          
            scope.$watch("currentProperty.value", function(newValue) {
                if (formCtrl.$invalid) {
                    scope.errorMsg = "";
                    formCtrl.$setValidity('valPropertyMsg', true);                    
                }
            });
            
            //listen for server validation changes
            // NOTE: we pass in "" in order to listen for all validation changes to the content property, not for
            // validation changes to fields in the property this is because some server side validators may not
            // return the field name for which the error belongs too, just the property for which it belongs.
            serverValidationService.subscribe(scope.currentProperty, "", function (isValid, propertyErrors, allErrors) {
                hasError = !isValid;
                if (hasError) {
                    //set the error message to the server message
                    scope.errorMsg = propertyErrors[0].errorMsg;                                     
                    //now that we've used the server validation message, we need to remove it from the 
                    //error collection... it is a 'one-time' usage so that when the field is invalidated 
                    //again, the message we display is the client side message.
                    //NOTE: 'this' in the subscribe callback context is the validation manager object.
                    this.removeError(scope.currentProperty);
                    //flag that the current validator is invalid
                    formCtrl.$setValidity('valPropertyMsg', false);
                }
                else {
                    scope.errorMsg = "";
                    //flag that the current validator is valid
                    formCtrl.$setValidity('valPropertyMsg', true);                                 
                }
            });
            
            //when the element is disposed we need to unsubscribe!
            // NOTE: this is very important otherwise when this controller re-binds the previous subscriptsion will remain
            // but they are a different callback instance than the above.
            element.bind('$destroy', function () {
                serverValidationService.unsubscribe(scope.currentProperty, "");
            });
        }
    };
}
angular.module('umbraco.directives').directive("valPropertyMsg", valPropertyMsg);