define(['angular', 'app'], function(angular, app) {

    return app.config(function ($routeProvider) {
        $routeProvider
        .when('/:section', {
            templateUrl: function (rp) {
                //$log.log(rp.section);
                return "views/common/dashboard.html";
            }
        })
        .when('/:section/:method', {
            templateUrl: function(rp) {
                //$log.log(rp.section);

                if (!rp.method)
                    return "views/common/dashboard.html";
                
                return 'views/' + rp.section + '/' + rp.method + '.html';
            }
        })
        .when('/:section/:method/:id', {
            templateUrl: function(rp) {
                //$log.log(rp.section);

                if (!rp.method)
                    return "views/common/dashboard.html";
                
                return 'views/' + rp.section + '/' + rp.method + '.html';
            }
        })
        .otherwise({
            redirectTo: function (rp) {
                //$log.log(rp.section);

                return '/content'
            }
        });
    }).config(function ($locationProvider) {
    //$locationProvider.html5Mode(false).hashPrefix('!'); //turn html5 mode off
    // $locationProvider.html5Mode(true);         //turn html5 mode on
});

});
