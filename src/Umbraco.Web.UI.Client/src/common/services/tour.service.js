/**
 @ngdoc service
 * @name umbraco.services.tourService
 *
 * @description
 * <b>Added in Umbraco 7.8</b>. Application-wide service for handling tours.
 */
(function () {
    'use strict';

    function tourService(eventsService, localStorageService) {

        var localStorageKey = "umbTours";
        var tours = [];
        var currentTour = null;        

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#registerTour
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Registers a tour in the service
		 * @param {Object} tour The tour you want to register in the service
         * @param {String} tour.name The tour name
         * @param {String} tour.alias The tour alias
         * @param {Array} tour.steps Array of tour steps
         * @param {String} tour.step.title Step title
         * @param {DomElement} tour.step.content Step content (pass in any HTML markup)
         * @param {DomElement} tour.step.element Highlight a DOM-element
         * @param {Boolean} tour.step.elementPreventClick Adds invisible layer on top of highligted element to prevent all clicks and interaction with it
         * @param {Number} tour.step.backdropOpacity Sets the backdrop opacity (default 0.4)
		 */
        function registerTour(newTour) {
            tours.push(newTour);
            eventsService.emit("appState.tour.updatedTours", tours);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#registerTours
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Registers an array of tours in the service
		 * @param {Array} tours The tours to register in the service
		 */
        function registerTours(newTours) {
            angular.forEach(newTours, function(newTour){
                tours.push(newTour);
            });
            eventsService.emit("appState.tour.updatedTours", tours);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#unregisterTour
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Unregisters a tour in the service
		 * @param {String} tourAlias The tour alias of the tour you want to unregister
		 */
        function unregisterTour(tourAlias) {
            tours = tours.filter(function( obj ) {
                return obj.alias !== tourAlias;
            });
            eventsService.emit("appState.tour.updatedTours", tours);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#unregisterTourGroup
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Unregisters a tour in the service
		 * @param {String} tourGroupName The name of the tour group you want to unregister
		 */
        function unregisterTourGroup(tourGroup) {
            tours = tours.filter(function( obj ) {
                return obj.group !== tourGroup;
            });
            eventsService.emit("appState.tour.updatedTours", tours);
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#startTour
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Raises an event to start a tour
		 * @param {Object} tour The tour which should be started
		 */
        function startTour(tour) {
            eventsService.emit("appState.tour.start", tour);
            currentTour = tour;            
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#endTour
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Raises an event to end the current tour
		 */
        function endTour() {
            eventsService.emit("appState.tour.end");
            currentTour = null;            
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#completeTour
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Raises an event to complete the current tour and saves the completed tour alias in local storage
         * @param {Object} tour The tour which should be completed
		 */
        function completeTour(tour) {
            saveInLocalStorage(tour);
            eventsService.emit("appState.tour.complete", tour);
            currentTour = null;            
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#getCurrentTour
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Returns the current tour
         * @returns {Array} Returns the current tour
		 */
        function getCurrentTour() {
            return currentTour;
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#getAllTours
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Returns all registered tours from the service
         * @returns {Array} All registered tours
		 */
        function getAllTours() {
            setCompletedTours();
            return tours;
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#getGroupedTours
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Returns all registered tours grouped by tour group
         * @returns {Array} All registered tours grouped by tour group
		 */
        function getGroupedTours() {
            var  groupedTours = {};
            if(tours && tours.length > 0) {
                setCompletedTours();
                groupedTours = _.groupBy(tours, "group");
            }
            return groupedTours;
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#getTourByAlias
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Returns a tour with the passed in alias
         * @param {Object} tourAlias The tour alias of the tour which should be returned
         * @returns {Object} Tour object
		 */
        function getTourByAlias(tourAlias) {
            var tour = _.findWhere(tours, {alias: tourAlias});
            return tour;
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#getCompletedTours
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Returns an array of the completed tour aliases
         * @returns {Array} Array of completed tour aliases
		 */
        function getCompletedTours() {
            var completedTours = localStorageService.get(localStorageKey);
            var aliases = _.pluck(completedTours, "alias");
            return aliases;
        }

        ///////////

        function setCompletedTours() {

            var storedTours = [];

            if (localStorageService.get(localStorageKey)) {
                storedTours = localStorageService.get(localStorageKey);
            }

            angular.forEach(storedTours, function (storedTour) {
                if (storedTour.completed === true) {
                    angular.forEach(tours, function (tour) {
                        if (storedTour.alias === tour.alias) {
                            tour.completed = true;
                        }
                    });
                }
            });

        }

        function saveInLocalStorage(tour) {
            var storedTours = [];
            var tourFound = false;

            if (localStorageService.get(localStorageKey)) {
                storedTours = localStorageService.get(localStorageKey);
            }

            if (storedTours.length > 0) {
                angular.forEach(storedTours, function (storedTour) {
                    if (storedTour.alias === tour.alias) {
                        storedTour.completed = true;
                        tourFound = true;
                    }
                });
            }

            if (!tourFound) {
                var storageObject = {
                    "alias": tour.alias,
                    "completed": true
                };
                storedTours.push(storageObject);
            }

            localStorageService.set(localStorageKey, storedTours);

        }

        var service = {
            registerTour: registerTour,
            registerTours: registerTours,
            unregisterTour: unregisterTour,
            unregisterTourGroup: unregisterTourGroup,
            startTour: startTour,
            endTour: endTour,
            completeTour: completeTour,
            getCurrentTour: getCurrentTour,            
            getAllTours: getAllTours,
            getGroupedTours: getGroupedTours,
            getTourByAlias: getTourByAlias,
            getCompletedTours: getCompletedTours
        };

        return service;

    }

    angular.module("umbraco.services").factory("tourService", tourService);

})();
