app.config(function ($routeProvider) {
    $routeProvider
        .when('/:section', {
            templateUrl: function (rp) {
                if (rp.section === "default")
                {
                    rp.section = "content";
                }

                rp.url = "dashboard.aspx?app=" + rp.section;
                return 'views/common/legacy.html';
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
                
                return 'views/' + rp.section + '/' + rp.method + '.html';
            }
        })
        .when('/:section/:method/:id', {
            templateUrl: function (rp) {
                if (!rp.method)
                    return "views/common/dashboard.html";

                return 'views/' + rp.section + '/' + rp.method + '.html';
            }
        })        
        .otherwise({ redirectTo: '/content' });
    }).config(function ($locationProvider) {

        //$locationProvider.html5Mode(false).hashPrefix('!'); //turn html5 mode off
        // $locationProvider.html5Mode(true);         //turn html5 mode on
});


app.run(['userService', function (userService) {
    // Get the current user when the application starts
    // (in case they are still logged in from a previous session)
    userService.isAuthenticated();
}]);  
