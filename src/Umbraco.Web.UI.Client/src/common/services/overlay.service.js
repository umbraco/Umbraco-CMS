/**
 @ngdoc service
 * @name umbraco.services.overlayService
 *
 * @description
 * <b>Added in Umbraco 8.0</b>. Application-wide service for handling overlays.
 */
(function () {
    "use strict";

    function overlayService(eventsService, backdropService) {

        var currentOverlay = null;

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
            backdropService.open(backdropOptions);
            currentOverlay = overlay;
            eventsService.emit("appState.overlay", overlay);
        }

        function close() {
            backdropService.close();
            currentOverlay = null;
            eventsService.emit("appState.overlay", null);
        }

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

        function confirm(overlay) {

            if (!overlay.closeButtonLabelKey) overlay.closeButtonLabelKey = "general_cancel";
            if (!overlay.view) overlay.view = "views/common/overlays/confirm/confirm.html";
            if (!overlay.close) overlay.close = function () { close(); };

            switch (overlay.confirmType) {

                case "delete":
                    if (!overlay.confirmMessageStyle) overlay.confirmMessageStyle = "danger";
                    if (!overlay.submitButtonStyle) overlay.submitButtonStyle = "danger";
                    if (!overlay.submitButtonLabelKey) overlay.submitButtonLabelKey = "contentTypeEditor_yesDelete";
                    break;
                
                default:
                    if (!overlay.submitButtonLabelKey) overlay.submitButtonLabelKey = "general_confirm";

            }

            open(overlay);
        }

        function confirmDelete(overlay) {
            overlay.confirmType = "delete";
            confirm(overlay);
        }

        var service = {
            open: open,
            close: close,
            ysod: ysod,
            confirm: confirm,
            confirmDelete: confirmDelete
        };

        return service;

    }

    angular.module("umbraco.services").factory("overlayService", overlayService);


})();
