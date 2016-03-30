/**
 * @ngdoc service
 * @name umbraco.services.formHelper
 * @function
 *
 * @description
 * A utility class used to streamline how forms are developed, to ensure that validation is check and displayed consistently and to ensure that the correct events
 * fire when they need to.
 */
function formHelper(angularHelper, serverValidationManager, $timeout, notificationsService, dialogService, localizationService) {
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
            //if no statusPropertyName is set we'll default to formStatus.
            if (!args.statusPropertyName) {
                args.statusPropertyName = "formStatus";
            }
            //if no statusTimeout is set, we'll  default to 2500 ms
            if (!args.statusTimeout) {
                args.statusTimeout = 2500;
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
            
            //check if a form status should be set on the scope
            if (args.statusMessage) {
                args.scope[args.statusPropertyName] = args.statusMessage;

                //clear the message after the timeout
                $timeout(function () {
                    args.scope[args.statusPropertyName] = undefined;
                }, args.statusTimeout);
            }

            return true;
        },
        
        /**
         * @ngdoc function
         * @name umbraco.services.formHelper#submitForm
         * @methodOf umbraco.services.formHelper
         * @function
         *
         * @description
         * Called by controllers when a form has been successfully submitted. the correct events execute 
         * and that the notifications are displayed if there are any.
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
            
            //if no statusPropertyName is set we'll default to formStatus.
            if (!args.statusPropertyName) {
                args.statusPropertyName = "formStatus";
            }
            //clear the status
            args.scope[args.statusPropertyName] = null;

            if (angular.isArray(args.notifications)) {
                for (var i = 0; i < args.notifications.length; i++) {
                    notificationsService.showNotification(args.notifications[i]);
                }
            }

            args.scope.$broadcast("formSubmitted", { scope: args.scope });
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
            //When the status is a 400 status with a custom header: X-Status-Reason: Validation failed, we have validation errors.
            //Otherwise the error is probably due to invalid data (i.e. someone mucking around with the ids or something).
            //Or, some strange server error
            if (err.status === 400) {
                //now we need to look through all the validation errors
                if (err.data && (err.data.ModelState)) {

                    //wire up the server validation errs
                    this.handleServerValidation(err.data.ModelState);

                    //execute all server validation events and subscribers
                    serverValidationManager.executeAndClearAllSubscriptions();                    
                }
                else {
                    dialogService.ysodDialog(err);
                }
            }
            else {
                dialogService.ysodDialog(err);
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
                // So, to do this (since we need to support backwards compat), we need to hack a little bit. For Content Properties,
                // which are user defined, we know that they will exist with a prefixed ModelState of "_Properties.", so if we detect
                // this, then we know it's a Property.

                //the alias in model state can be in dot notation which indicates
                // * the first part is the content property alias
                // * the second part is the field to which the valiation msg is associated with
                //There will always be at least 2 parts for properties since all model errors for properties are prefixed with "Properties"
                //If it is not prefixed with "Properties" that means the error is for a field of the object directly.

                var parts = e.split(".");

                //Check if this is for content properties - specific to content/media/member editors because those are special 
                // user defined properties with custom controls.
                if (parts.length > 1 && parts[0] === "_Properties") {

                    var propertyAlias = parts[1];

                    //if it contains 2 '.' then we will wire it up to a property's field
                    if (parts.length > 2) {
                        //add an error with a reference to the field for which the validation belongs too
                        serverValidationManager.addPropertyError(propertyAlias, parts[2], modelState[e][0]);
                    }
                    else {
                        //add a generic error for the property, no reference to a specific field
                        serverValidationManager.addPropertyError(propertyAlias, "", modelState[e][0]);
                    }

                }
                else {

                    //Everthing else is just a 'Field'... the field name could contain any level of 'parts' though, for example:
                    // Groups[0].Properties[2].Alias
                    serverValidationManager.addFieldError(e, modelState[e][0]);
                }

                //add to notifications
                notificationsService.error("Validation", modelState[e][0]);

            }
        }
    };
}
angular.module('umbraco.services').factory('formHelper', formHelper);