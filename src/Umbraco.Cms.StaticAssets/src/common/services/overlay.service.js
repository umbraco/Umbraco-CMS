/**
 @ngdoc service
 * @name umbraco.services.overlayService
 *
 * @description
 * <b>Added in Umbraco 8.0</b>. Application-wide service for handling overlays.
 */
(function () {
    "use strict";

    function overlayService(eventsService, backdropService, focusLockService) {

        let currentOverlay = null;

        /**
         * @ngdoc method
         * @name umbraco.services.overlayService#open
         * @methodOf umbraco.services.overlayService
         *
         * @description
         * Opens a new overlay.
         *
         * @param {object} overlay The rendering options for the overlay.
         * @param {string=} overlay.view The URL to the view. Defaults to `views/common/overlays/default/default.html` if nothing is specified.
         * @param {string=} overlay.position The alias of the position of the overlay. Defaults to `center`.
         * 
         * Custom positions can be added by adding a CSS rule for the the underlying CSS rule. Eg. for the position `center`, the corresponding `umb-overlay-center` CSS rule is defined as:
         * 
         * <pre>
         * .umb-overlay.umb-overlay-center {
         *     position: absolute;
         *     width: 600px;
         *     height: auto;
         *     top: 50%;
         *     left: 50%;
         *     transform: translate(-50%,-50%);
         *     border-radius: 3px;
         * }
         * </pre>
         * @param {string=} overlay.size Sets an alias for the size of the overlay to be opened. If set to `small` (default), an `umb-overlay--small` class name will be appended the the class list of the main overlay element in the DOM.
         * 
         * Umbraco does not support any more sizes by default, but if you wish to introduce a `medium` size, you could do so by adding a CSS rule simlar to:
         * 
         * <pre>
         * .umb-overlay-center.umb-overlay--medium {
         *     width: 800px;
         * }
         * </pre>
         * @param {booean=} overlay.disableBackdropClick A boolean value indicating whether the click event on the backdrop should be disabled.
         * @param {string=} overlay.title The overall title of the overlay. The title will be omitted if not specified.
         * @param {string=} overlay.subtitle The sub title of the overlay. The sub title will be omitted if not specified.
         * @param {object=} overlay.itemDetails An item that will replace the header of the overlay.
         * @param {string=} overlay.itemDetails.icon The icon of the item - eg. `icon-book`.
         * @param {string=} overlay.itemDetails.title The title of the item.
         * @param {string=} overlay.itemDetails.description Sets the description of the item.         * 
         * @param {string=} overlay.submitButtonLabel The label of the submit button. To support localized values, it's recommended to use the `submitButtonLabelKey` instead.
         * @param {string=} overlay.submitButtonLabelKey The key to be used for the submit button label. Defaults to `general_submit` if not specified.
         * @param {string=} overlay.submitButtonState The state of the submit button. Possible values are inherited from the [umbButton directive](#/api/umbraco.directives.directive:umbButton) and are `init`, `busy", `success`, `error`.
         * @param {string=} overlay.submitButtonStyle The styling of the submit button. Possible values are inherited from the [umbButton directive](#/api/umbraco.directives.directive:umbButton) and are `primary`, `info`, `success`, `warning`, `danger`, `inverse`, `link` and `block`. Defaults to `success` if not specified specified.
         * @param {string=} overlay.hideSubmitButton A boolean value indicating whether the submit button should be hidden. Default is `false`.
         * @param {string=} overlay.disableSubmitButton A boolean value indicating whether the submit button should be disabled, preventing the user from submitting the overlay. Default is `false`.
         * @param {string=} overlay.closeButtonLabel The label of the close button. To support localized values, it's recommended to use the `closeButtonLabelKey` instead.
         * @param {string=} overlay.closeButtonLabelKey The key to be used for the close button label. Defaults to `general_close` if not specified.
         * @param {string=} overlay.submit A callback function that is invoked when the user submits the overlay.
         * @param {string=} overlay.close A callback function that is invoked when the user closes the overlay.
         */
        function open(newOverlay) {

            // prevent two open overlays at the same time
            if (currentOverlay) {
                close();
            }

            var backdropOptions = {};
            var overlay = newOverlay;

            // set the default overlay position to center
            if (!overlay.position) {
                overlay.position = "center";
            }

            // set the default overlay size to small
            if (!overlay.size) {
                overlay.size = "small";
            }

            // use a default empty view if nothing is set
            if (!overlay.view) {
                overlay.view = "views/common/overlays/default/default.html";
            }

            // option to disable backdrop clicks
            if (overlay.disableBackdropClick) {
                backdropOptions.disableEventsOnClick = true;
            }

            overlay.show = true;
            focusLockService.addInertAttribute();
            backdropService.open(backdropOptions);
            currentOverlay = overlay;
            eventsService.emit("appState.overlay", overlay);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.overlayService#close
         * @methodOf umbraco.services.overlayService
         *
         * @description
         * Closes the current overlay.
         */
        function close() {
            focusLockService.removeInertAttribute();

            var tourIsOpen = document.body.classList.contains("umb-tour-is-visible");
            if (!tourIsOpen) {
                backdropService.close();
            }
            
            currentOverlay = null;
            eventsService.emit("appState.overlay", null);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.overlayService#ysod
         * @methodOf umbraco.services.overlayService
         *
         * @description
         * Opens a new overlay with an error message.
         *
         * @param {object} error The error to be shown.
         */
        function ysod(error) {
            const overlay = {
                view: "views/common/overlays/ysod/ysod.html",
                error: error,
                close: function() {
                    close();
                }
            };
            open(overlay);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.overlayService#confirm
         * @methodOf umbraco.services.overlayService
         *
         * @description
         * Opens a new overlay prompting the user to confirm the overlay.
         *
         * @param {object} overlay The options for the overlay.
         * @param {string=} overlay.confirmType The type of the confirm dialog, which helps define standard styling and labels of the overlay. Supported values are `delete` and `remove`.
         * @param {string=} overlay.closeButtonLabelKey The key to be used for the cancel button label. Defaults to `general_cancel` if not specified.
         * @param {string=} overlay.view The URL to the view. Defaults to `views/common/overlays/confirm/confirm.html` if nothing is specified.
         * @param {string=} overlay.confirmMessageStyle The styling of the confirm message. If `overlay.confirmType` is `delete`, the fallback value is `danger` - otherwise a message style isn't explicitly specified.
         * @param {string=} overlay.submitButtonStyle The styling of the confirm button. Possible values are inherited from the [umbButton directive](#/api/umbraco.directives.directive:umbButton) and are `primary`, `info`, `success`, `warning`, `danger`, `inverse`, `link` and `block`.
         * 
         * If not specified, the fallback value depends on the value specified for the `overlay.confirmType` parameter:
         * 
         * - `delete`: fallback key is `danger`
         * - `remove`: fallback key is `primary`
         * - anything else: no fallback AKA default button style
         * @param {string=} overlay.submitButtonLabelKey The key to be used for the confirm button label. 
         * 
         * If not specified, the fallback value depends on the value specified for the `overlay.confirmType` parameter:
         * 
         * - `delete`: fallback key is `actions_delete`
         * - `remove`: fallback key is `actions_remove`
         * - anything else: fallback is `general_confirm`
         * @param {function=} overlay.close A callback function that is invoked when the user closes the overlay.
         * @param {function=} overlay.submit A callback function that is invoked when the user confirms the overlay.
         */
        function confirm(overlay) {

            if (!overlay.closeButtonLabelKey) overlay.closeButtonLabelKey = "general_cancel";
            if (!overlay.view) overlay.view = "views/common/overlays/confirm/confirm.html";
            if (!overlay.close) overlay.close = function () { close(); };

            switch (overlay.confirmType) {

                case "delete":
                    if (!overlay.confirmMessageStyle) overlay.confirmMessageStyle = "danger";
                    if (!overlay.submitButtonStyle) overlay.submitButtonStyle = "danger";
                    if (!overlay.submitButtonLabelKey) overlay.submitButtonLabelKey = "actions_delete";
                    break;

                case "remove":
                    if (!overlay.submitButtonStyle) overlay.submitButtonStyle = "primary";
                    if (!overlay.submitButtonLabelKey) overlay.submitButtonLabelKey = "actions_remove";
                    break;
                
                default:
                    if (!overlay.submitButtonLabelKey) overlay.submitButtonLabelKey = "general_confirm";

            }

            open(overlay);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.overlayService#confirmDelete
         * @methodOf umbraco.services.overlayService
         *
         * @description
         * Opens a new overlay prompting the user to confirm the overlay. The overlay will have styling and labels useful for when the user needs to confirm a delete action.
         *
         * @param {object} overlay The options for the overlay.
         * @param {string=} overlay.closeButtonLabelKey The key to be used for the cancel button label. Defaults to `general_cancel` if not specified.
         * @param {string=} overlay.view The URL to the view. Defaults to `views/common/overlays/confirm/confirm.html` if nothing is specified.
         * @param {string=} overlay.confirmMessageStyle The styling of the confirm message. Defaults to `delete` if not specified specified.
         * @param {string=} overlay.submitButtonStyle The styling of the confirm button. Possible values are inherited from the [umbButton directive](#/api/umbraco.directives.directive:umbButton) and are `primary`, `info`, `success`, `warning`, `danger`, `inverse`, `link` and `block`. Defaults to `danger` if not specified specified.
         * @param {string=} overlay.submitButtonLabelKey The key to be used for the confirm button label. Defaults to `actions_delete` if not specified.
         * @param {function=} overlay.close A callback function that is invoked when the user closes the overlay.
         * @param {function=} overlay.submit A callback function that is invoked when the user confirms the overlay.
         */
        function confirmDelete(overlay) {
            overlay.confirmType = "delete";
            confirm(overlay);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.overlayService#confirmRemove
         * @methodOf umbraco.services.overlayService
         *
         * @description
         * Opens a new overlay prompting the user to confirm the overlay. The overlay will have styling and labels useful for when the user needs to confirm a remove action.
         *
         * @param {object} overlay The options for the overlay.
         * @param {string=} overlay.closeButtonLabelKey The key to be used for the cancel button label. Defaults to `general_cancel` if not specified.
         * @param {string=} overlay.view The URL to the view. Defaults to `views/common/overlays/confirm/confirm.html` if nothing is specified.
         * @param {string=} overlay.confirmMessageStyle The styling of the confirm message - eg. `danger`.
         * @param {string=} overlay.submitButtonStyle The styling of the confirm button. Possible values are inherited from the [umbButton directive](#/api/umbraco.directives.directive:umbButton) and are `primary`, `info`, `success`, `warning`, `danger`, `inverse`, `link` and `block`. Defaults to `primary` if not specified specified.
         * @param {string=} overlay.submitButtonLabelKey The key to be used for the confirm button label. Defaults to `actions_remove` if not specified.
         * @param {function=} overlay.close A callback function that is invoked when the user closes the overlay.
         * @param {function=} overlay.submit A callback function that is invoked when the user confirms the overlay.
         */
        function confirmRemove(overlay) {
            overlay.confirmType = "remove";
            confirm(overlay);
        }

        var service = {
            open: open,
            close: close,
            ysod: ysod,
            confirm: confirm,
            confirmDelete: confirmDelete,
            confirmRemove: confirmRemove
        };

        return service;

    }

    angular.module("umbraco.services").factory("overlayService", overlayService);


})();
