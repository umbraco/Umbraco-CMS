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
            return tourResource.getTours().then(function(tourFiles) {
                angular.forEach(tourFiles, function (tourFile) {
                    angular.forEach(tourFile, function(newTour) {
                        validateTour(newTour);
                        validateTourRegistration(newTour);
                        tours.push(newTour);    
                    });
                });
                eventsService.emit("appState.tour.updatedTours", tours);
            });
        }

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
            validateTour(newTour);
            validateTourRegistration(newTour);
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
                validateTour(newTour);
                validateTourRegistration(newTour);
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
                    function() {
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
                    function() {
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
            //TODO: This should be reset if a new user logs in
            return currentTour;
        }

        /**
         * @ngdoc method
         * @name umbraco.services.tourService#getAllTours
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Returns a promise of all tours with the current user statuses
         * @returns {Array} All registered tours
		 */
        function getAllTours() {
            var tours = getTours();
            return setTourStatuses(tours);
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
            setTourStatuses(tours).then(function() {
                var groupedTours = _.groupBy(tours, "group");
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
         * @name umbraco.services.tourService#getCompletedTours
         * @methodOf umbraco.services.tourService
         *
         * @description
         * Returns a promise of completed tours for the user
         * @returns {Array} Array of completed tour aliases
		 */
        function getCompletedTours() {
            var deferred = $q.defer();
            currentUserResource.getTours().then(function (storedTours) {
                var completedTours = _.where(storedTours, { completed: true });
                var aliases = _.pluck(completedTours, "alias");
                deferred.resolve(aliases);
            });
            return deferred.promise;
        }

        /**
         * Returns a promise of disabled tours for the user
         */
        function getDisabledTours() {
            var deferred = $q.defer();
            currentUserResource.getTours().then(function (storedTours) {
                var disabledTours = _.where(storedTours, { disabled: true });
                var aliases = _.pluck(disabledTours, "alias");
                deferred.resolve(aliases);
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

        }
        
        /**
         * Validates a tour before it gets registered in the service
         * @param {any} tour
         */
        function validateTourRegistration(tour) {
            // check for existing tours with the same alias
            angular.forEach(tours, function (existingTour) {
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

                angular.forEach(storedTours, function (storedTour) {
                    if (storedTour.completed === true) {
                        angular.forEach(tours, function (tour) {
                            if (storedTour.alias === tour.alias) {
                                tour.completed = true;
                            }
                        });
                    }
                    if (storedTour.disabled === true) {
                        angular.forEach(tours, function (tour) {
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

        function saveInLocalStorage(tour) {
            var storedTours = [];
            var tourFound = false;

            // check if something exists in local storage
            if (localStorageService.get(localStorageKey)) {
                storedTours = localStorageService.get(localStorageKey);
            }

            // update existing tour in localstorage if it's already there
            if (storedTours.length > 0) {
                angular.forEach(storedTours, function (storedTour) {
                    if (storedTour.alias === tour.alias) {
                        storedTour.completed = storedTour.completed ? storedTour.completed : tour.completed;
                        storedTour.disabled = storedTour.disabled ? storedTour.disabled : tour.disabled;
                        tourFound = true;
                    }
                });
            }

            // create new entry in local storage
            if (!tourFound) {
                var storageObject = {
                    "alias": tour.alias,
                    "completed": tour.completed,
                    "disabled": tour.disabled
                };
                storedTours.push(storageObject);
            }

            localStorageService.set(localStorageKey, storedTours);

        }

        var service = {
            registerAllTours: registerAllTours,
            registerTour: registerTour,
            registerTours: registerTours,
            unregisterTour: unregisterTour,
            unregisterTourGroup: unregisterTourGroup,
            startTour: startTour,
            endTour: endTour,
            disableTour: disableTour,
            completeTour: completeTour,
            getCurrentTour: getCurrentTour,
            //TODO: Not used
            getAllTours: getAllTours,
            getGroupedTours: getGroupedTours,
            getTourByAlias: getTourByAlias,
            //TODO: Not used
            getCompletedTours: getCompletedTours,
            //TODO: Not used
            getDisabledTours: getDisabledTours,
        };

        return service;

    }

    angular.module("umbraco.services").factory("tourService", tourService);

})();
