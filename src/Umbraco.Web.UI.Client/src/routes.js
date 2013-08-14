app.config(function ($routeProvider) {
    $routeProvider
        .when('/framed/:url', {
            //This occurs when we need to launch some content in an iframe
            templateUrl: function (rp) {
                if (!rp.url)
                    throw "A framed resource must have a url route parameter";
                return 'views/common/legacy.html';
            }
        })
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
        .when('/:section/:tree', {
            templateUrl: function (rp) {
                if (rp.section === "default")
                {
                    rp.section = "content";
                }

                if (rp.tree === "")
                {
                    rp.tree = "default";
                }

                rp.url = "dashboard.aspx?app=" + rp.section;
                return 'views/common/legacy.html';
            }
        })
        .when('/:section/:tree/:method', {
            templateUrl: function(rp) {
                if (!rp.method){
                    return "views/common/dashboard.html";
                }
                
                if(rp.tree === "default" || rp.tree === ""){
                    return 'views/' + rp.section + '/' + rp.method + '.html';
                }else{
                    return 'views/' + rp.section + '/' + rp.tree + '/' + rp.method + '.html';
                }
            }
        })
        .when('/:section/:tree/:method/:id', {
            templateUrl: function (rp) {
                if (!rp.method)
                    return "views/common/dashboard.html";

                if(rp.tree === "default" || rp.tree === ""){
                    return 'views/' + rp.section + '/' + rp.method + '.html';
                }else{
                    return 'views/' + rp.section + '/' + rp.tree + '/' + rp.method + '.html';
                }
            }
        })
        .otherwise({ redirectTo: '/content/document' });
    }).config(function ($locationProvider) {

        //$locationProvider.html5Mode(false).hashPrefix('!'); //turn html5 mode off
        // $locationProvider.html5Mode(true);         //turn html5 mode on
});


app.run(['userService', function (userService) {
    // Get the current user when the application starts
    // (in case they are still logged in from a previous session)
    userService.isAuthenticated();
}]);  
