(function () {
    'use strict';

    function tourService(eventsService) {

        function startTour(tour) {
            eventsService.emit("appState.startTour", tour);
        }

        function endTour() {
            eventsService.emit("appState.endTour");
        }

        function completeTour() {
            eventsService.emit("appState.endTour");
        }

        var service = {
          startTour: startTour,
          endTour: endTour,
          completeTour: completeTour
        };

        return service;

    }

    angular.module('umbraco.services').factory('tourService', tourService);

})();
