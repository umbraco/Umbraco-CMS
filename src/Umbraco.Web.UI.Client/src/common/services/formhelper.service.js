/**
 * @ngdoc service
 * @name umbraco.services.formHelper
 * @function
 *
 * @description
 * A utility class used to streamline how forms are developed, to ensure that validation is check and displayed consistently and to ensure that the correct events
 * fire when they need to.
 */
function formHelper(angularHelper, serverValidationManager, notificationsService, overlayService) {
    return {

        /**
         * @ngdoc function
         * @name umbraco.services.formHelper#submitForm
         * @methodOf umbraco.services.formHelper
         * @function
         *
         * @description
         * Called by controllers when submitting a form - this ensures that all client validation is checked,
         * server validation is cleared, that the correct events execute and status messages are displayed.
         * This returns true if the form is valid, otherwise false if form submission cannot continue.
         *
         * @param {object} args An object containing arguments for form submission
         */
        submitForm: function (args) {

            var currentForm;

            if (!args) {
                throw "args cannot be null";
            }
            if (!args.scope) {
                throw "args.scope cannot be null";
            }

            if (!args.formCtrl) {
                //try to get the closest form controller
                currentForm = angularHelper.getRequiredCurrentForm(args.scope);
            }
            else {
                currentForm = args.formCtrl;
            }

            //the first thing any form must do is broadcast the formSubmitting event
            args.scope.$broadcast("formSubmitting", { scope: args.scope, action: args.action });

            this.focusOnFirstError(currentForm);

            // Some property editors need to perform an action after all property editors have reacted to the formSubmitting.
            args.scope.$broadcast("formSubmittingFinalPhase", { scope: args.scope, action: args.action });

            // Set the form state to submitted
            currentForm.$setSubmitted();

            //then check if the form is valid
            if (!args.skipValidation) {
                if (currentForm.$invalid) {

                    return false;
                }
            }

            //reset the server validations if required (default is true), otherwise notify existing ones of changes
            if (!args.keepServerValidation) {
                serverValidationManager.reset();
            }
            else {
                serverValidationManager.notify();
            }

            return true;
        },

         /**
         * @ngdoc function
         * @name umbraco.services.formHelper#focusOnFirstError
         * @methodOf umbraco.services.formHelper
         * @function
         *
         * @description
         * Called by submitForm when a form has been submitted, it will fire a focus on the first found invalid umb-property it finds in the form..
         *
         * @param {object} form Pass in a form object.
         */
        focusOnFirstError: function(form) {
            var invalidNgForms = form.$$element.find(`.umb-property ng-form.ng-invalid, .umb-property-editor ng-form.ng-invalid-required`);
            var firstInvalidNgForm = invalidNgForms.first();

            if(firstInvalidNgForm.length !== 0) {
                var focusableFields = [...firstInvalidNgForm.find("umb-range-slider .noUi-handle,input,textarea,select,button")];
                if(focusableFields.length !== 0) {
                    var firstErrorEl = focusableFields.find(el => el.type !== "hidden" && el.hasAttribute("readonly") === false);
                    if(firstErrorEl !== undefined) {
                        firstErrorEl.focus();
                    }
                }
            }
        },

        /**
         * @ngdoc function
         * @name umbraco.services.formHelper#submitForm
         * @methodOf umbraco.services.formHelper
         * @function
         *
         * @description
         * Called by controllers when a form has been successfully submitted, this ensures the correct events are raised.
         *
         * @param {object} args An object containing arguments for form submission
         */
        resetForm: function (args) {

            var currentForm;

            if (!args) {
                throw "args cannot be null";
            }
            if (!args.scope) {
                throw "args.scope cannot be null";
            }
            if (!args.formCtrl) {
                //try to get the closest form controller
                currentForm = angularHelper.getRequiredCurrentForm(args.scope);
            }
            else {
                currentForm = args.formCtrl;
            }

            // Set the form state to pristine
            currentForm.$setPristine();
            currentForm.$setUntouched();

            args.scope.$broadcast(args.hasErrors ? "formSubmittedValidationFailed" : "formSubmitted", { scope: args.scope });
        },

        showNotifications: function (args) {
            if (!args || !args.notifications) {
                return false;
            }
            if (Utilities.isArray(args.notifications)) {
                for (var i = 0; i < args.notifications.length; i++) {
                    notificationsService.showNotification(args.notifications[i]);
                }
                return true;
            }
            return false;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.formHelper#handleError
         * @methodOf umbraco.services.formHelper
         * @function
         *
         * @description
         * Needs to be called when a form submission fails, this will wire up all server validation errors in ModelState and
         * add the correct messages to the notifications. If a server error has occurred this will show a ysod.
         *
         * @param {object} err The error object returned from the http promise
         */
        handleError: function (err) {

            //TODO: Potentially add in the logic to showNotifications like the contentEditingHelper.handleSaveError does so that
            // non content editors can just use this method instead of contentEditingHelper.handleSaveError which they should not use
            // and they won't need to manually do it.

            //When the status is a 400 status with a custom header: X-Status-Reason: Validation failed, we have validation errors.
            //Otherwise the error is probably due to invalid data (i.e. someone mucking around with the ids or something).
            //Or, some strange server error
            if (err.status === 400) {
                //now we need to look through all the validation errors
                if (err.data && err.data.ModelState) {

                    //wire up the server validation errs
                    this.handleServerValidation(err.data.ModelState);

                    //execute all server validation events and subscribers
                    serverValidationManager.notifyAndClearAllSubscriptions();
                }
            }
            else {

                // TODO: All YSOD handling should be done with an interceptor
                overlayService.ysod(err);
            }

        },

        /**
         * @ngdoc function
         * @name umbraco.services.formHelper#handleServerValidation
         * @methodOf umbraco.services.formHelper
         * @function
         *
         * @description
         * This wires up all of the server validation model state so that valServer and valServerField directives work
         *
         * @param {object} err The error object returned from the http promise
         */
        handleServerValidation: function (modelState) {
            serverValidationManager.addErrorsForModelState(modelState);
        }
    };
}
angular.module('umbraco.services').factory('formHelper', formHelper);
