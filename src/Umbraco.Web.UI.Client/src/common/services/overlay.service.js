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

        function open(overlay) {

            var backdropOptions = {};

            // set the default overlay position to center
            if(!overlay.position) {
                overlay.position = "center";
            }

            // option to disable backdrop clicks
            if(overlay.disableBackdropClick) {
                backdropOptions.disableEventsOnClick = true;
            }

            overlay.show = true;
            backdropService.open(backdropOptions);
            eventsService.emit("appState.overlay", overlay);
        }

        function close() {
            backdropService.close();
            eventsService.emit("appState.overlay", null);
        }

        var service = {
            open: open,
            close: close
        };

        return service;

    }

    angular.module("umbraco.services").factory("overlayService", overlayService);

})();