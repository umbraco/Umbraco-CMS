(function () {
    "use strict";

    function backdropService(eventsService) {

        var args = {
            element: null,
            show: false
        };

        function open(backdrop) {

            if(backdrop && backdrop.element) {
                args.element = backdrop.element;
            }

            args.show = true;

            eventsService.emit("appState.backdrop", args);
        }

        function close() {

            args.element = null;
            args.show = false;

            eventsService.emit("appState.backdrop", args);
        }

        function setHighlight(element) {
            args.element = element;
            eventsService.emit("appState.backdrop", args);
        }

        var service = {
            open: open,
            close: close,
            setHighlight: setHighlight
        };

        return service;

    }

    angular.module("umbraco.services").factory("backdropService", backdropService);

})();
