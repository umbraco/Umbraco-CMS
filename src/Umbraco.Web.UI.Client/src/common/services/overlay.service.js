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
            if(!overlay.position) {
                overlay.position = "center";
            }
            overlay.show = true;
            backdropService.open();
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