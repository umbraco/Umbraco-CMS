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

            //then check if the form is valid
            if (!args.skipValidation) {
                if (currentForm.$invalid) {
                    return false;
                }
            }

            //reset the server validations
            serverValidationManager.reset();

            return true;
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
            if (!args) {
                throw "args cannot be null";
            }
            if (!args.scope) {
                throw "args.scope cannot be null";
            }

            args.scope.$broadcast("formSubmitted", { scope: args.scope });
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
            for (var e in modelState) {

                //This is where things get interesting....
                // We need to support validation for all editor types such as both the content and content type editors.
                // The Content editor ModelState is quite specific with the way that Properties are validated especially considering
                // that each property is a User Developer property editor.
                // The way that Content Type Editor ModelState is created is simply based on the ASP.Net validation data-annotations 
                // system. 
                // So, to do this there's some special ModelState syntax we need to know about.
                // For Content Properties, which are user defined, we know that they will exist with a prefixed
                // ModelState of "_Properties.", so if we detect this, then we know it's for a content Property.

                //the alias in model state can be in dot notation which indicates
                // * the first part is the content property alias
                // * the second part is the field to which the valiation msg is associated with
                //There will always be at least 4 parts for content properties since all model errors for properties are prefixed with "_Properties"
                //If it is not prefixed with "_Properties" that means the error is for a field of the object directly.

                // Example: "_Properties.headerImage.en-US.mySegment.myField"
                // * it's for a property since it has a _Properties prefix
                // * it's for the headerImage property type
                // * it's for the en-US culture
                // * it's for the mySegment segment
                // * it's for the myField html field (optional)

                var parts = e.split(".");

                //Check if this is for content properties - specific to content/media/member editors because those are special 
                // user defined properties with custom controls.
                if (parts.length > 1 && parts[0] === "_Properties") {

                    var propertyAlias = parts[1];

                    var culture = null;
                    if (parts.length > 2) {
                        culture = parts[2];
                        //special check in case the string is formatted this way
                        if (culture === "null") {
                            culture = null;
                        }
                    }

                    var segment = null;
                    if (parts.length > 3) {
                        segment = parts[3];
                        //special check in case the string is formatted this way
                        if (segment === "null") {
                            segment = null;
                        }
                    }

                    var htmlFieldReference = "";
                    if (parts.length > 4) {
                        htmlFieldReference = parts[4] || "";
                    }

                    // add a generic error for the property
                    serverValidationManager.addPropertyError(propertyAlias, culture, htmlFieldReference, modelState[e][0], segment);

                } else {

                    //Everthing else is just a 'Field'... the field name could contain any level of 'parts' though, for example:
                    // Groups[0].Properties[2].Alias
                    serverValidationManager.addFieldError(e, modelState[e][0]);
                }

            }
        }
    };
}
angular.module('umbraco.services').factory('formHelper', formHelper);
