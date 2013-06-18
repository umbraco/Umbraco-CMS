app.config(function ($routeProvider) {
        $routeProvider
        .when('/:section', {
            templateUrl: "views/common/dashboard.html"
        })
        .when('/:section/:method', {
            templateUrl: function(rp) {
                if (!rp.method)
                    return "views/common/dashboard.html";
                
                return 'views/' + rp.section + '/' + rp.method + '.html';
            }
        })
        .when('/:section/:method/:id', {
            templateUrl: function(rp) {
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