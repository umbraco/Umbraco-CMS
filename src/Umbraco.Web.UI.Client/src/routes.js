app.config(function ($routeProvider) {
    $routeProvider
        .when('/:section', {
            templateUrl: function (rp) {
                if (rp.section === "default" || rp.section === "")
                {
                    rp.section = "content";
                }

                rp.url = "dashboard.aspx?app=" + rp.section;
                return 'views/common/dashboard.html';
            }
        })
        .when('/framed/:url', {
            //This occurs when we need to launch some content in an iframe
            templateUrl: function (rp) {
                if (!rp.url)
                    throw "A framed resource must have a url route parameter";

                return 'views/common/legacy.html';
            }
        })
        .when('/:section/:method', {
            templateUrl: function(rp) {
                if (!rp.method)
                    return "views/common/dashboard.html";
                
                //NOTE: This current isn't utilized by anything but does open up some cool opportunities for
                // us since we'll be able to have specialized views for individual sections which is something
                // we've never had before. So could utilize this for a new dashboard model when we get native
                // angular dashboards working. Perhaps a normal section dashboard would list out the registered
                // dashboards (as tabs if we wanted) and each tab could actually be a route link to one of these views?

                return 'views/' + rp.section + '/' + rp.method + '.html';
            }
        })
        .when('/:section/:tree/:method/:id', {
            templateUrl: function (rp) {
                if (!rp.tree || !rp.method) {
                    return "views/common/dashboard.html";
                }
                    
                //we don't need to put views into section folders since theoretically trees
                // could be moved among sections, we only need folders for specific trees.
                return 'views/' + rp.tree + '/' + rp.method + '.html';
            }
        })        
        .otherwise({ redirectTo: '/content' });
    }).config(function ($locationProvider) {

        //$locationProvider.html5Mode(false).hashPrefix('!'); //turn html5 mode off
        // $locationProvider.html5Mode(true);         //turn html5 mode on
});


app.run(['userService', '$log', '$rootScope', function (userService, $log, $rootScope) {

    // Get the current user when the application starts
    // (in case they are still logged in from a previous session)

    userService.isAuthenticated({broadcastEvent: true});
}]);  
