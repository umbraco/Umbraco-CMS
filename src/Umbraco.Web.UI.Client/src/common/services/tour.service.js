/**
 @ngdoc service
 * @name umbraco.services.tourService
 *
 * @description
 * <b>Added in Umbraco 7.8</b>. Application-wide service for handling tours.
 */
(function () {
    'use strict';

    function tourService(eventsService, currentUserResource, $q, tourResource) {

        var tours = [];
        var currentTour = null;

        /**
         * Registers all tours from the server and returns a promise
         */
        function registerAllTours() {
            tours = [];
            return tourResource.getTours().then(function (tourFiles) {
                Utilities.forEach(tourFiles, tourFile => {

                    Utilities.forEach(tourFile.tours, newTour => {
                        validateTour(newTour);
                        validateTourRegistration(newTour);
                        tours.push(newTour);
                    });
                });

                eventsService.emit("appState.tour.updatedTours", tours);
            });
        }

        /**
         * Method to return all of the tours as a new instance
         */
        function getTours() {
            return tours;
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
            validateTour(tour);
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
        function endTour(tour) {
            eventsService.emit("appState.tour.end", tour);
            currentTour = null;
        }

        /**
         * Disables a tour for the user, raises an event and returns a promise
         * @param {any} tour
         */
        function disableTour(tour) {
            var deferred = $q.defer();
            tour.disabled = true;
            currentUserResource
                .saveTourStatus({ alias: tour.alias, disabled: tour.disabled, completed: tour.completed }).then(
                    function () {
                        eventsService.emit("appState.tour.end", tour);
                        currentTour = null;
                        deferred.resolve(tour);
                    });
            return deferred.promise;
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#completeTour
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Completes a tour for the user, raises an event and returns a promise
         * @param {Object} tour The tour which should be completed
         */
        function completeTour(tour) {
            var deferred = $q.defer();
            tour.completed = true;
            currentUserResource
                .saveTourStatus({ alias: tour.alias, disabled: tour.disabled, completed: tour.completed }).then(
                    function () {
                        eventsService.emit("appState.tour.complete", tour);
                        currentTour = null;
                        deferred.resolve(tour);
                    });
            return deferred.promise;
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#getCurrentTour
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Returns the current tour
         * @returns {Object} Returns the current tour
         */
        function getCurrentTour() {
            // TODO: This should be reset if a new user logs in
            return currentTour;
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#getGroupedTours
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Returns a promise of grouped tours with the current user statuses
         * @returns {Array} All registered tours grouped by tour group
         */
        function getGroupedTours() {
            var deferred = $q.defer();
            var tours = getTours();
            setTourStatuses(tours).then(function () {
                var groupedTours = [];
                tours.forEach(function (item) {

                    if (item.contentType === null || item.contentType === '') {
                        var groupExists = false;
                        var newGroup = {
                            "group": "",
                            "tours": []
                        };

                        groupedTours.forEach(function (group) {
                            // extend existing group if it is already added
                            if (group.group === item.group) {
                                if (item.groupOrder) {
                                    group.groupOrder = item.groupOrder;
                                }
                                groupExists = true;

                                if (item.hidden === false) {
                                    group.tours.push(item);
                                }
                            }
                        });

                        // push new group to array if it doesn't exist
                        if (!groupExists) {
                            newGroup.group = item.group;
                            if (item.groupOrder) {
                                newGroup.groupOrder = item.groupOrder;
                            }

                            if (item.hidden === false) {
                                newGroup.tours.push(item);
                                groupedTours.push(newGroup);
                            }
                        }
                    }

                });

                deferred.resolve(groupedTours);
            });
            return deferred.promise;
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#getTourByAlias
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Returns a promise of the tour found by alias with the current user statuses
         * @param {Object} tourAlias The tour alias of the tour which should be returned
         * @returns {Object} Tour object
         */
        function getTourByAlias(tourAlias) {
            var deferred = $q.defer();
            var tours = getTours();
            setTourStatuses(tours).then(function () {
                var tour = _.findWhere(tours, { alias: tourAlias });
                deferred.resolve(tour);
            });
            return deferred.promise;
        }

        /**
        * @ngdoc method
        * @name umbraco.services.tourService#getToursForDoctype
        * @methodOf umbraco.services.tourService
        *
        * @description
        * Returns a promise of the tours found by documenttype alias.
        * @param {Object} doctypeAlias The doctype alias for which  the tours which should be returned
        * @returns {Array} An array of tour objects for the doctype
        */
        function getToursForDoctype(doctypeAlias) {
            var deferred = $q.defer();
            tourResource.getToursForDoctype(doctypeAlias).then(function (tours) {
                deferred.resolve(tours);
            });
            return deferred.promise;
        }

        ///////////

        /**
         * Validates a tour object and makes sure it consists of the correct properties needed to start a tour
         * @param {any} tour
         */
        function validateTour(tour) {

            if (!tour) {
                throw "A tour is not specified";
            }

            if (!tour.alias) {
                throw "A tour alias is required";
            }

            if (!tour.steps) {
                throw "Tour " + tour.alias + " is missing tour steps";
            }

            if (tour.steps && tour.steps.length === 0) {
                throw "Tour " + tour.alias + " is missing tour steps";
            }

            if (tour.requiredSections.length === 0) {
                throw "Tour " + tour.alias + " is missing the required sections";
            }
        }

        /**
         * Validates a tour before it gets registered in the service
         * @param {any} tour
         */
        function validateTourRegistration(tour) {
            // check for existing tours with the same alias
            Utilities.forEach(tours, existingTour => {
                if (existingTour.alias === tour.alias) {
                    throw "A tour with the alias " + tour.alias + " is already registered";
                }
            });
        }

        /**
         * Based on the tours given, this will set each of the tour statuses (disabled/completed) based on what is stored against the current user
         * @param {any} tours
         */
        function setTourStatuses(tours) {

            var deferred = $q.defer();
            currentUserResource.getTours().then(function (storedTours) {

                Utilities.forEach(storedTours, storedTour => {

                    if (storedTour.completed === true) {
                        Utilities.forEach(tours, tour => {
                            if (storedTour.alias === tour.alias) {
                                tour.completed = true;
                            }
                        });
                    }
                    if (storedTour.disabled === true) {
                        Utilities.forEach(tours, tour => {
                            if (storedTour.alias === tour.alias) {
                                tour.disabled = true;
                            }
                        });
                    }
                });

                deferred.resolve(tours);
            });
            return deferred.promise;
        }

        var service = {
            registerAllTours: registerAllTours,
            startTour: startTour,
            endTour: endTour,
            disableTour: disableTour,
            completeTour: completeTour,
            getCurrentTour: getCurrentTour,
            getGroupedTours: getGroupedTours,
            getTourByAlias: getTourByAlias,
            getToursForDoctype: getToursForDoctype
        };

        return service;

    }

    angular.module("umbraco.services").factory("tourService", tourService);

})();
