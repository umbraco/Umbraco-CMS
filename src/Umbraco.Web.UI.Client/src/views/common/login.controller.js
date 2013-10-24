/** This controller is simply here to launch the login dialog when the route is explicitly changed to /login */
angular.module('umbraco').controller("Umbraco.LoginController", function ($scope, userService, $location) {

    userService._showLoginDialog();
    
    //when a user is authorized redirect - this will only be handled here when we are actually on the /login route
    $scope.$on("authenticated", function(evt, data) {
    	//reset the avatar
        $scope.avatar = "assets/img/application/logo.png";
        $location.path("/").search("");
    });

});