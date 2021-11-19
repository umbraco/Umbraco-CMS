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
            var hasError = false; // tracks if there is a child error or an explicit error

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
            var currentSegment = currentProperty.segment;

            // validation object won't exist when editor loads outside the content form (ie in settings section when modifying a content type)
            var isMandatory = currentProperty.validation ? currentProperty.validation.mandatory : undefined;

            var labels = {};
            var showValidation = false;

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
                    var err = serverValidationManager.getPropertyError(umbPropCtrl.getValidationPath(), null, "", null);
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

            // check the current errors in the form (and recursive sub forms), if there is 1 or 2 errors
            // we can check if those are valPropertyMsg or valServer and can clear our error in those cases.
            function checkAndClearError() {

                var errCount = angularHelper.countAllFormErrors(formCtrl);

                if (errCount === 0) {
                    return true;
                }

                if (errCount > 2) {
                    return false;
                }

                var hasValServer = Utilities.isArray(formCtrl.$error.valServer);
                if (errCount === 1 && hasValServer) {
                    return true;
                }

                var hasOwnErr = hasExplicitError();
                if ((errCount === 1 && hasOwnErr) || (errCount === 2 && hasOwnErr && hasValServer)) {

                    var propertyValidationPath = umbPropCtrl.getValidationPath();
                    // check if we can clear it based on child server errors, if we are the only explicit one remaining we can clear ourselves
                    if (isLastServerError(propertyValidationPath)) {
                        serverValidationManager.removePropertyError(propertyValidationPath, currentCulture, "", currentSegment);
                        return true;
                    }
                    return false;
                }

                return false;
            }

            // returns true if there is an explicit valPropertyMsg validation error on the form
            function hasExplicitError() {
                return Utilities.isArray(formCtrl.$error.valPropertyMsg);
            }

            // returns true if there is only a single server validation error for this property validation key in it's validation path
            function isLastServerError(propertyValidationPath) {
                var nestedErrs = serverValidationManager.getPropertyErrorsByValidationPath(
                    propertyValidationPath,
                    currentCulture,
                    currentSegment,
                    { matchType: "prefix" });
                if (nestedErrs.length === 0 || (nestedErrs.length === 1 && nestedErrs[0].propertyAlias === propertyValidationPath)) {

                    return true;
                }
                return false;
            }

            // a custom $validator function called on when each child ngModelController changes a value.
            function resetServerValidityValidator(fieldController) {
                const storedFieldController = fieldController; // pin a reference to this
                return (modelValue, viewValue) => {
                    // if the ngModelController value has changed, then we can check and clear the error
                    if (storedFieldController.$dirty) {
                        if (checkAndClearError()) {
                            resetError();
                        }
                    }
                    return true; // this validator is always 'valid'
                };
            }

            // collect all ng-model controllers recursively within the umbProperty form 
            // until it reaches the next nested umbProperty form
            function collectAllNgModelControllersRecursively(controls, ngModels) {
                controls.forEach(ctrl => {
                    if (angularHelper.isForm(ctrl)) {
                        // if it's not another umbProperty form then recurse
                        if (ctrl.$name !== formCtrl.$name) {
                            collectAllNgModelControllersRecursively(ctrl.$getControls(), ngModels);
                        }
                    }
                    else if (ctrl.hasOwnProperty('$modelValue') && Utilities.isObject(ctrl.$validators)) {
                        ngModels.push(ctrl);
                    }
                });
            }

            // We start the watch when there's server validation errors detected.
            // We watch on the current form's properties and on first watch or if they are dynamically changed
            // we find all ngModel controls recursively on this form (but stop recursing before we get to the next)
            // umbProperty form). Then for each ngModelController we assign a new $validator. This $validator 
            // will execute whenever the value is changed which allows us to check and reset the server validator
            function startWatch() {
                if (!watcher) {

                    watcher = scope.$watchCollection(
                        () => formCtrl,
                        function (updatedFormController) {
                            let childControls = updatedFormController.$getControls();
                            let ngModels = [];
                            collectAllNgModelControllersRecursively(childControls, ngModels);
                            ngModels.forEach(x => {
                                if (!x.$validators.serverValidityResetter) {
                                    x.$validators.serverValidityResetter = resetServerValidityValidator(x);
                                }
                            });
                        });
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
                stopWatch();
                hasError = false;
                formCtrl.$setValidity('valPropertyMsg', true);
                scope.errorMsg = "";

            }

            // This deals with client side validation changes and is executed anytime validators change on the containing 
            // valFormManager. This allows us to know when to display or clear the property error data for non-server side errors.
            function checkValidationStatus() {
                if (formCtrl.$invalid) {
                    //first we need to check if the valPropertyMsg validity is invalid
                    if (formCtrl.$error.valPropertyMsg && formCtrl.$error.valPropertyMsg.length > 0) {
                        //since we already have an error we'll just return since this means we've already set the 
                        //hasError and errorMsg properties which occurs below in the serverValidationManager.subscribe
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

            function onInit() {

                localizationService.localize("errors_propertyHasErrors").then(function (data) {

                    labels.propertyHasErrors = data;

                    //if there's any remaining errors in the server validation service then we should show them.
                    showValidation = serverValidationManager.items.length > 0;
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
                    // TODO: Really? Since valFormManager is watching a countof all errors which is more overhead than watching formCtrl.$invalid
                    // and there's a TODO there that it should just watch formCtrl.$invalid
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

                    if (scope.currentProperty) { //this can be null if no property was assigned

                        // listen for server validation changes for property validation path prefix.
                        // We pass in "" in order to listen for all validation changes to the content property, not for
                        // validation changes to fields in the property this is because some server side validators may not
                        // return the field name for which the error belongs too, just the property for which it belongs.
                        // It's important to note that we need to subscribe to server validation changes here because we always must
                        // indicate that a content property is invalid at the property level since developers may not actually implement
                        // the correct field validation in their property editors.

                        function serverValidationManagerCallback(isValid, propertyErrors, allErrors) {
                            var hadError = hasError;
                            hasError = !isValid;
                            if (hasError) {
                                //set the error message to the server message
                                scope.errorMsg = propertyErrors.length > 1 ? labels.propertyHasErrors : propertyErrors[0].errorMsg || labels.propertyHasErrors;
                                //flag that the current validator is invalid
                                formCtrl.$setValidity('valPropertyMsg', false);
                                startWatch();

                                // This check is required in order to be able to reset ourselves and is typically for complex editor
                                // scenarios where the umb-property itself doesn't contain any ng-model controls which means that the
                                // above serverValidityResetter technique will not work to clear valPropertyMsg errors.
                                // In order for this to work we rely on the current form controller's $pristine state. This means that anytime
                                // the form is submitted whether there are validation errors or not the state must be reset... this is automatically
                                // taken care of with the formHelper.resetForm method that should be used in all cases. $pristine is required because it's
                                // a value that is cascaded to all form controls based on the hierarchy of child ng-model controls. This allows us to easily
                                // know if a value has changed. The alternative is what we used to do which was to put a deep $watch on the entire complex value
                                // which is hugely inefficient. 
                                if (propertyErrors.length === 1 && hadError && !formCtrl.$pristine) {
                                    var propertyValidationPath = umbPropCtrl.getValidationPath();
                                    serverValidationManager.removePropertyError(propertyValidationPath, currentCulture, "", currentSegment);
                                    resetError();
                                }
                            }
                            else {
                                resetError();
                            }
                        }

                        unsubscribe.push(serverValidationManager.subscribe(
                            umbPropCtrl.getValidationPath(),
                            currentCulture,
                            "",
                            serverValidationManagerCallback,
                            currentSegment,
                            { matchType: "prefix" } // match property validation path prefix
                        ));
                    }

                });
            }

            //when the scope is disposed we need to unsubscribe
            scope.$on('$destroy', function () {
                stopWatch();
                unsubscribe.forEach(u => u());
            });

            onInit();
        }


    };
}
angular.module('umbraco.directives.validation').directive("valPropertyMsg", valPropertyMsg);
