(function () {
    "use strict";

    function backdropService(eventsService) {

        var args = {
            opacity: null,
            element: null,
            disableEventsOnClick: false,
            show: false
        };

        function open(options) {

            if(options && options.element) {
                args.element = options.element;
            }

            if(options && options.disableEventsOnClick) {
                args.disableEventsOnClick = options.disableEventsOnClick;
            }

            args.show = true;

            eventsService.emit("appState.backdrop", args);
        }

        function close() {
            args.element = null;
            args.show = false;
            eventsService.emit("appState.backdrop", args);
        }

        function setOpacity(opacity) {
            args.opacity = opacity;
            eventsService.emit("appState.backdrop", args);
        }

        function setHighlight(element) {
            args.element = element;
            eventsService.emit("appState.backdrop", args);
        }

        var service = {
            open: open,
            close: close,
            setOpacity: setOpacity,
            setHighlight: setHighlight
        };

        return service;

    }

    angular.module("umbraco.services").factory("backdropService", backdropService);

})();
