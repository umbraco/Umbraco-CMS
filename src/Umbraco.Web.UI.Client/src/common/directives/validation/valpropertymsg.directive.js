/**
* @ngdoc directive
* @name umbraco.directives.directive:valPropertyMsg
* @restrict A
* @element textarea
* @requires formController
* @description This directive is used to control the display of the property level validation message.
* We will listen for server side validation changes
* and when an error is detected for this property we'll show the error message.
* In order for this directive to work, the valFormManager directive must be placed on the containing form.
**/
function valPropertyMsg(serverValidationManager) {

    return {
        require: ['^^form', '^^valFormManager', '^^umbProperty', '?^^umbVariantContent'],
        replace: true,
        restrict: "E",
        template: "<div ng-show=\"errorMsg != ''\" class='alert alert-error property-error' >{{errorMsg}}</div>",
        scope: {},
        link: function (scope, element, attrs, ctrl) {
            
            var unsubscribe = [];
            var watcher = null;
            var hasError = false;

            //create properties on our custom scope so we can use it in our template
            scope.errorMsg = "";
            
            
            //the property form controller api
            var formCtrl = ctrl[0];
            //the valFormManager controller api
            var valFormManager = ctrl[1];
            //the property controller api
            var umbPropCtrl = ctrl[2];
            //the variants controller api
            var umbVariantCtrl = ctrl[3];
            
            var currentProperty = umbPropCtrl.property;
            scope.currentProperty = currentProperty;
            var currentCulture = currentProperty.culture;            
            var currentVariant = umbVariantCtrl.editor.content;
            
            // Lets check if we have variants and we are on the default language then ...
            if(umbVariantCtrl && umbVariantCtrl.content.variants.length > 1 && !currentVariant.language.isDefault && !currentCulture && !currentProperty.unlockInvariantValue) {
                //This property is locked cause its a invariant property shown on a non-default language.
                //Therefor do not validate this field.
                return;
            }
            
            // if we have reached this part, and there is no culture, then lets fallback to invariant. To get the validation feedback for invariant language.
            currentCulture = currentCulture || "invariant";
            

            // Gets the error message to display
            function getErrorMsg() {
                //this can be null if no property was assigned
                if (scope.currentProperty) {
                    //first try to get the error msg from the server collection
                    var err = serverValidationManager.getPropertyError(scope.currentProperty.alias, null, "");
                    //if there's an error message use it
                    if (err && err.errorMsg) {
                        return err.errorMsg;
                    }
                    else {
                        // TODO: localize
                        return scope.currentProperty.propertyErrorMessage ? scope.currentProperty.propertyErrorMessage : "Property has errors";
                    }

                }
                // TODO: localize
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
                    watcher = scope.$watch("currentProperty.value",
                        function (newValue, oldValue) {
                            
                            if (angular.equals(newValue, oldValue)) {
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

                            if (errCount === 0 || (errCount === 1 && angular.isArray(formCtrl.$error.valPropertyMsg)) || (formCtrl.$invalid && angular.isArray(formCtrl.$error.valServer))) {
                                scope.errorMsg = "";
                                formCtrl.$setValidity('valPropertyMsg', true);
                            } else if (showValidation && scope.errorMsg === "") {
                                formCtrl.$setValidity('valPropertyMsg', false);
                                scope.errorMsg = getErrorMsg();
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

            function checkValidationStatus() {
                if (formCtrl.$invalid) {
                    //first we need to check if the valPropertyMsg validity is invalid
                    if (formCtrl.$error.valPropertyMsg && formCtrl.$error.valPropertyMsg.length > 0) {
                        //since we already have an error we'll just return since this means we've already set the 
                        // hasError and errorMsg properties which occurs below in the serverValidationManager.subscribe
                        return;
                    }
                    //if there are any errors in the current property form that are not valPropertyMsg
                    else if (_.without(_.keys(formCtrl.$error), "valPropertyMsg").length > 0) {
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
            }

            //if there's any remaining errors in the server validation service then we should show them.
            var showValidation = serverValidationManager.items.length > 0;
            if (!showValidation) {
                //We can either get the form submitted status by the parent directive valFormManager (if we add a property to it)
                //or we can just check upwards in the DOM for the css class (easier for now).
                //The initial hidden state can't always be hidden because when we switch variants in the content editor we cannot
                //reset the status.
                showValidation = element.closest(".show-validation").length > 0;
            }


            //listen for form validation changes.
            //The alternative is to add a watch to formCtrl.$invalid but that would lead to many more watches then
            // subscribing to this single watch.
            valFormManager.onValidationStatusChanged(function (evt, args) {
                checkValidationStatus();
            });

            //listen for the forms saving event
            unsubscribe.push(scope.$on("formSubmitting", function (ev, args) {
                showValidation = true;
                if (hasError && scope.errorMsg === "") {
                    scope.errorMsg = getErrorMsg();
                    startWatch();
                }
                else if (!hasError) {
                    scope.errorMsg = "";
                    stopWatch();
                }
            }));

            //listen for the forms saved event
            unsubscribe.push(scope.$on("formSubmitted", function (ev, args) {
                showValidation = false;
                scope.errorMsg = "";
                formCtrl.$setValidity('valPropertyMsg', true);
                stopWatch();
            }));

            //listen for server validation changes
            // NOTE: we pass in "" in order to listen for all validation changes to the content property, not for
            // validation changes to fields in the property this is because some server side validators may not
            // return the field name for which the error belongs too, just the property for which it belongs.
            // It's important to note that we need to subscribe to server validation changes here because we always must
            // indicate that a content property is invalid at the property level since developers may not actually implement
            // the correct field validation in their property editors.

            if (scope.currentProperty) { //this can be null if no property was assigned
                unsubscribe.push(serverValidationManager.subscribe(scope.currentProperty.alias,
                    currentCulture,
                    "",
                    function(isValid, propertyErrors, allErrors) {
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
                    }));

            }

            //when the scope is disposed we need to unsubscribe
            scope.$on('$destroy', function () {
                stopWatch();
                for (var u in unsubscribe) {
                    unsubscribe[u]();
                }
            });
        }


    };
}
angular.module('umbraco.directives.validation').directive("valPropertyMsg", valPropertyMsg);
