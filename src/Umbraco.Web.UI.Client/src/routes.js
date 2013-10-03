app.config(function ($routeProvider) {

    /** This checks if the user is authenticated for a route and what the isRequired is set to. 
        Depending on whether isRequired = true, it first check if the user is authenticated and will resolve successfully
        otherwise the route will fail and the $routeChangeError event will execute, in that handler we will redirect to the rejected
        path that is resolved from this method and prevent default (prevent the route from executing) */
    var checkAuth = function(isRequired) {
        return {
            isAuthenticated: function ($q, userService, $route) {
                var deferred = $q.defer();

                //don't need to check if we've redirected to login and we've already checked auth
                if (!$route.current.params.section && $route.current.params.check === false) {
                    deferred.resolve(true);
                    return deferred.promise;
                }
                
                userService.isAuthenticated()
                    .then(function () {
                        if (isRequired) {
                            //this will resolve successfully so the route will continue
                            deferred.resolve(true);
                        }
                        else {
                            deferred.reject({ path: "/" });
                        }
                    }, function () {
                        if (isRequired) {                            
                            //the check=false is checked above so that we don't have to make another http call to check
                            //if they are logged in since we already know they are not.
                            deferred.reject({ path: "/login", search: { check: false } });
                        }
                        else {
                            //this will resolve successfully so the route will continue
                            deferred.resolve(true);
                        }
                    });                
                return deferred.promise;
            }
        };
    };

    $routeProvider
        .when('/login', {
            templateUrl: 'views/common/login.html',
            //ensure auth is *not* required so it will redirect to /content otherwise
            resolve: checkAuth(false)
        })
        .when('/:section', {
            templateUrl: function (rp) {
                if (rp.section.toLowerCase() === "default" || rp.section.toLowerCase() === "umbraco" || rp.section === "")
                {
                    rp.section = "content";
                }

                rp.url = "dashboard.aspx?app=" + rp.section;
                return 'views/common/dashboard.html';
            },
            resolve: checkAuth(true)
        })
        .when('/framed/:url', {
            //This occurs when we need to launch some content in an iframe
            templateUrl: function (rp) {
                if (!rp.url)
                    throw "A framed resource must have a url route parameter";

                return 'views/common/legacy.html';
            },
            resolve: checkAuth(true)
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
            },
            resolve: checkAuth(true)
        })
        .when('/:section/:tree/:method/:id', {
            //This allows us to dynamically change the template for this route since you cannot inject services into the templateUrl method.
            template: "<div ng-include='templateUrl'></div>",
            //This controller will execute for this route, then we replace the template dynamnically based on the current tree.
            controller: function ($scope, $route, $routeParams, treeService) {

                if (!$routeParams.tree || !$routeParams.method) {
                    $scope.templateUrl = "views/common/dashboard.html";
                }

                // Here we need to figure out if this route is for a package tree and if so then we need
                // to change it's convention view path to:
                // /App_Plugins/{mypackage}/umbraco/{treetype}/{method}.html
                
                // otherwise if it is a core tree we use the core paths:
                // views/{treetype}/{method}.html

                var packageTreeFolder = treeService.getTreePackageFolder($routeParams.tree);

                if (packageTreeFolder) {
                    $scope.templateUrl = Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath +
                        "/" + packageTreeFolder +
                        "/umbraco/" + $routeParams.tree + "/" + $routeParams.method + ".html";
                }
                else {
                    $scope.templateUrl = 'views/' + $routeParams.tree + '/' + $routeParams.method + '.html';
                }
                
            },            
            resolve: checkAuth(true)
        })        
        .otherwise({ redirectTo: '/login' });
    }).config(function ($locationProvider) {

        //$locationProvider.html5Mode(false).hashPrefix('!'); //turn html5 mode off
        // $locationProvider.html5Mode(true);         //turn html5 mode on
});


app.run(['userService', '$log', '$rootScope', '$location', function (userService, $log, $rootScope, $location) {

    var firstRun = true;

    /** when we have a successful first route that is not the login page - meaning the user is authenticated
        we'll get the current user from the user service and ensure it broadcasts it's events. If the route
        is successful from after a login then this will not actually do anything since the authenticated event would
        have alraedy fired, but if the user is loading the angularjs app for the first time and they are already authenticated
        then this is when the authenticated event will be fired.
    */
    $rootScope.$on('$routeChangeSuccess', function (event, current, previous) {
        if (firstRun && !$location.url().toLowerCase().startsWith("/login")) {
            firstRun = false;
            userService.getCurrentUser({ broadcastEvent: true });
        }
    });

    /** When the route change is rejected - based on checkAuth - we'll prevent the rejected route from executing including
        wiring up it's controller, etc... and then redirect to the rejected URL.   */
    $rootScope.$on('$routeChangeError', function (event, current, previous, rejection) {
        event.preventDefault();
        $location.path(rejection.path).search(rejection.search);
    });

}]);  
