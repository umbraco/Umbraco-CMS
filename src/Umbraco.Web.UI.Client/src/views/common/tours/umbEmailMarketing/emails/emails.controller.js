(function () {
    "use strict";

    function EmailsController($scope, eventsService, tourService, userService) {

        var vm = this;

        vm.optIn = function() {
            // Get the current user in backoffice
            userService.getCurrentUser().then(function(user){
                // Send this user along to opt in
                // It's a fire & forget - not sure we need to check the response
                userService.addUserToEmailMarketing(user);
            });

            // Mark Tour as complete
            // This is also can help us indicate that the user accepted
            // Where disabled is set if user closes modal or chooses NO
            $scope.model.completeTour();
        }

        // Listen for when all tours are closed
        eventsService.on("appState.tour.end", function (evt, data) {

            // Only do something if it is this hidden email marketing tour
            // The email tour was 'disabled' or 'completed' which both emit 'appstate.tour.end'
            if(data.alias === "umbEmailMarketing"){

                // Get the intro tour
                tourService.getTourByAlias("umbIntroIntroduction").then(function (introTour) {
                    // start intro tour if it hasn't been completed or disabled
                    if (introTour && introTour.disabled !== true && introTour.completed !== true) {
                        tourService.startTour(introTour);
                    }
                });
            }
        });
    }

    angular.module("umbraco").controller("Umbraco.Tours.UmbEmailMarketing.EmailsController", EmailsController);
})();
