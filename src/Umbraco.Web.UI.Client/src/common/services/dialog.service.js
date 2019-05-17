/**
 @ngdoc service
 * @name umbraco.services.dialogService
 *
 * @description
 * <b>Added in Umbraco 8.1</b>. Application-wide service for creating dialogs.
 */
(function () {
    "use strict";

    function dialogService(overlayService, localizationService) {

        function confirmDelete(cancelCallback, submitCallback, args) {
            if (!cancelCallback || !submitCallback) {
                console.error("dialogService.confirm requires callback methods for Cancel and Submit in args.cancelCallback and args.submitCallback respectively.")
                return;
            }
            localizationService.localizeMany([args.titleKey || "general_delete", args.contentKey || "defaultdialogs_confirmSure", args.cancelButtonKey || "general_cancel", args.submitButtonKey || "contentTypeEditor_yesDelete"])
                .then(function (data) {
                    const overlay = {
                        title: data[0],
                        content: data[1],
                        closeButtonLabel: data[2],
                        submitButtonLabel: data[3],
                        submitButtonStyle: args.submitButtonStyle || "danger",
                        close: function () {
                            cancelCallback();
                            overlayService.close();
                        },
                        submit: function () {
                            submitCallback();
                            overlayService.close();
                        }
                    };
                    overlayService.open(overlay);
                });
        }

        return {
            /**
             * @ngdoc method
             * @name umbraco.services.dialogService#confirmDelete
             * @methodOf umbraco.services.dialogService
             * @function
             *
             * @description
             * Opens a dialog to confirm deletion of items. Example of use:
             * 
             * dialogService.confirmDelete(
             *   // cancel dialog callback:
             *   function () {
             *     // handle when user cancels the dialog
             *   },
             *   // submit dialog callback:
             *   function () {
             *     // handle when user submits the dialog
             *   },
             *   // additional dialog args
             *   {
             *     titleKey: "my_customTitleLocalizationKey",
             *     contentKey: "my_customContentLocalizationKey"
             *   }
             * );
             * @param {function} cancelCallback the method to invoke if the dialog was cancelled by the user
             * @param {function} submitCallback the method to invoke if the dialog was submitted by the user
             * @param {object} args optional arguments for the dialog
             * @param {string} args.titleKey the localization key to use for the dialog title
             * @param {string} args.contentKey the localization key to use for the dialog content
             * @param {string} args.cancelButtonKey the localization key to use for the dialog cancel button
             * @param {string} args.submitButtonKey the localization key to use for the dialog submit button
             * @param {string} args.submitButtonStyle the style to apply to the the dialog submit button
             */
            confirmDelete: confirmDelete
        };

    }

    angular.module("umbraco.services").factory("dialogService", dialogService);

})();
