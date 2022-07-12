/** This controller is simply here to launch the login dialog when the route is explicitly changed to /login */
angular.module('umbraco').controller("Umbraco.LoginController", function (eventsService, $scope, userService, $location, $rootScope, $window) {

    userService._showLoginDialog();

    var evtOn = eventsService.on("app.ready", function(evt, data){
        $scope.avatar = "assets/img/application/logo.png";

        var path = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath;

      //check if there's a returnPath query string, if so redirect to it
        var locationObj = $location.search();
        if (locationObj.returnPath) {
            path = decodeURIComponent(locationObj.returnPath);
        }

        // Ensure path is not absolute
        path = path.replace(/^.*\/\/[^\/]+/, '')

        window.location.href = path;
    });

    $scope.$on('$destroy', function () {
        eventsService.unsubscribe(evtOn);
    });

});
