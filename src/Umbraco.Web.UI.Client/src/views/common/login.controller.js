/** This controller is simply here to launch the login dialog when the route is explicitly changed to /login */
angular.module('umbraco').controller("Umbraco.LoginController", function (eventsService, $scope, userService, $location) {

    userService._showLoginDialog(); 
       
    eventsService.on("app.ready", function(){
    	$scope.avatar = "assets/img/application/logo.png";
    	$location.path("/").search("");
    });
});