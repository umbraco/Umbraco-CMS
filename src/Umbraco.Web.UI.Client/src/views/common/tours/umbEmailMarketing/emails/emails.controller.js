(function () {
    "use strict";

    function EmailsController($scope, userService) {

        var vm = this;

        vm.optIn = function() {
            // Get the current user in backoffice
            userService.getCurrentUser().then(function(user){
                // Send this user along to opt in
                // It's a fire & forget - not sure we need to check the response
                userService.addUserToEmailMarketing(user);
            });

            $scope.model.nextStep();
        }
    }

    angular.module("umbraco").controller("Umbraco.Tours.UmbEmailMarketing.EmailsController", EmailsController);
})();
