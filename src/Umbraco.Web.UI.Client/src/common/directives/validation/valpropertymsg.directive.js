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
* We don't set the validity of this validator to false when client side validation fails, only when server side
* validation fails however we do respond to the client side validation changes to display error and adjust UI state.
**/
function valPropertyMsg(serverValidationManager, localizationService, angularHelper) {

    return {
        require: ['^^form', '^^valFormManager', '^^umbProperty', '?^^umbVariantContent', '?^^valPropertyMsg'],
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

            var propertyValidationKey = umbPropCtrl.getValidationPath();

            var currentCulture = currentProperty.culture;
            var currentSegment = currentProperty.segment;

            // validation object won't exist when editor loads outside the content form (ie in settings section when modifying a content type)
            var isMandatory = currentProperty.validation ? currentProperty.validation.mandatory : undefined;

            var labels = {};
            localizationService.localize("errors_propertyHasErrors").then(function (data) {
                labels.propertyHasErrors = data;
            });

            if (umbVariantCtrl) {
                //if we are inside of an umbVariantContent directive

                var currentVariant = umbVariantCtrl.editor.content;

                // Lets check if we have variants and we are on the default language then ...
                if (umbVariantCtrl.content.variants.length > 1 && (!currentVariant.language || !currentVariant.language.isDefault) && !currentCulture && !currentSegment && !currentProperty.unlockInvariantValue) {
                    //This property is locked cause its a invariant property shown on a non-default language.
                    //Therefor do not validate this field.
                    return;
                }
            }

            // if we have reached this part, and there is no culture, then lets fallback to invariant. To get the validation feedback for invariant language.
            currentCulture = currentCulture || "invariant";


            // Gets the error message to display
            function getErrorMsg() {
                //this can be null if no property was assigned
                if (scope.currentProperty) {
                    //first try to get the error msg from the server collection
                    var err = serverValidationManager.getPropertyError(propertyValidationKey, null, "", null);
                    //if there's an error message use it
                    if (err && err.errorMsg) {
                        return err.errorMsg;
                    }
                    else {
                        return scope.currentProperty.propertyErrorMessage ? scope.currentProperty.propertyErrorMessage : labels.propertyHasErrors;
                    }

                }
                return labels.propertyHasErrors;
            }

            // We need to subscribe to any changes to our model (based on user input)
            // This is required because when we have a server error we actually invalidate 
            // the form which means it cannot be resubmitted. 
            // So once a field is changed that has a server error assigned to it
            // we need to re-validate it for the server side validator so the user can resubmit
            // the form. Of course normal client-side validators will continue to execute. 
            function startWatch() {

                // TODO: Can we watch on something other than the value?? This doesn't work for complex editors especially once that have a 
                // viewmodel/model setup the value that this is watching doesn't actually get updated by a sub-editor in all cases.
                // we can probably watch the formCtrl view value? But then we also don't want this to watch complex values that have sub editors anyways
                // since that might end up clearing the whole chain of valPropertyMsg when a sub value is changed (in some cases, not with the block editor).

                //if there's not already a watch

                if (!watcher) {
                    watcher = scope.$watch("currentProperty.value",
                        function (newValue, oldValue) {
                            if (angular.equals(newValue, oldValue)) {
                                return;
                            }

                            var errCount = 0;

                            for (var e in formCtrl.$error) {
                                if (Utilities.isArray(formCtrl.$error[e])) {
                                    errCount++;
                                }
                            }

                            //we are explicitly checking for valServer errors here, since we shouldn't auto clear
                            // based on other errors. We'll also check if there's no other validation errors apart from valPropertyMsg, if valPropertyMsg
                            // is the only one, then we'll clear.

                            if (errCount === 0
                                || (errCount === 1 && Utilities.isArray(formCtrl.$error.valPropertyMsg))
                                || (formCtrl.$invalid && Utilities.isArray(formCtrl.$error.valServer))) {
                                resetError();
                            }
                            else if (showValidation && scope.errorMsg === "") {
                                formCtrl.$setValidity('valPropertyMsg', false, formCtrl);
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

            function resetError() {
                var hadError = hasError;
                hasError = false;
                formCtrl.$setValidity('valPropertyMsg', true, formCtrl);
                scope.errorMsg = "";
                stopWatch();

                // if we had an error, then check on the current valFormManager to see if it's 
                // now valid, if it is it means that the containing form (i.e. the form rendering)
                // properties for an element/content type) is now valid which means we can clear 
                // the parent's valPropertyMsg if there is one. This will only occur with complex editors
                // where we have nested umb-property components.
                if (hadError) {
                    scope.$evalAsync(function () {

                        // we need to navigate the parentForm here, unfortunately there's no real alternative unless we create our own directive
                        // of some sort but that would also get messy. This works though since in this case we're always going to be in the property
                        // form and the parent form about this will contain any invalid flags for all the other sibling properties. So when that is
                        // no longer invalid, we can check if we have a parent validation key (meaning we'd be nested inside of umb-property) and 
                        // we can clear that server error. 
                        // TODO: If there is another server error for this property though this might clear it inadvertently, at this time I'm unsure how to deal with that.
                        var parentValidationKey = umbPropCtrl.getParentValidationPath();
                        if (parentValidationKey) {
                            var parentForm = formCtrl.$$parentForm;
                            if (parentForm && !parentForm.$invalid) {
                                serverValidationManager.removePropertyError(parentValidationKey, currentCulture, "", currentSegment);
                            }
                        }
                        
                    });
                    
                    
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

                        // errors exist, but if the property is NOT mandatory and has no value, the errors should be cleared
                        if (isMandatory !== undefined && isMandatory === false && !currentProperty.value) {

                            resetError();

                            // if there's no value, the controls can be reset, which clears the error state on formCtrl
                            for (let control of formCtrl.$getControls()) {
                                control.$setValidity();
                            }

                            return;
                        }

                        hasError = true;
                        //update the validation message if we don't already have one assigned.
                        if (showValidation && scope.errorMsg === "") {
                            scope.errorMsg = getErrorMsg();
                        }
                    }
                    else {
                        resetError();
                    }
                }
                else {
                    resetError();
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
                    resetError();
                }
            }));

            //listen for the forms saved event
            unsubscribe.push(scope.$on("formSubmitted", function (ev, args) {
                showValidation = false;
                resetError();
            }));

            //listen for server validation changes
            // NOTE: we pass in "" in order to listen for all validation changes to the content property, not for
            // validation changes to fields in the property this is because some server side validators may not
            // return the field name for which the error belongs too, just the property for which it belongs.
            // It's important to note that we need to subscribe to server validation changes here because we always must
            // indicate that a content property is invalid at the property level since developers may not actually implement
            // the correct field validation in their property editors.

            if (scope.currentProperty) { //this can be null if no property was assigned, TODO: I don't believe it can? If it was null we'd get errors above

                function serverValidationManagerCallback(isValid, propertyErrors, allErrors) {
                    hasError = !isValid;
                    if (hasError) {
                        //set the error message to the server message
                        scope.errorMsg = propertyErrors[0].errorMsg ? propertyErrors[0].errorMsg : labels.propertyHasErrors;
                        //flag that the current validator is invalid
                        formCtrl.$setValidity('valPropertyMsg', false, formCtrl);
                        startWatch();
                    }
                    else {
                        resetError();
                    }
                }

                unsubscribe.push(serverValidationManager.subscribe(propertyValidationKey,
                    currentCulture,
                    "",
                    serverValidationManagerCallback,
                    currentSegment
                )
                );
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
