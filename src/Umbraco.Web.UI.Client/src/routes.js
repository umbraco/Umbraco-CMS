app.config(function ($routeProvider) {
    
    /** This checks if the user is authenticated for a route and what the isRequired is set to. 
        Depending on whether isRequired = true, it first check if the user is authenticated and will resolve successfully
        otherwise the route will fail and the $routeChangeError event will execute, in that handler we will redirect to the rejected
        path that is resolved from this method and prevent default (prevent the route from executing) */
    var canRoute = function(isRequired) {
        
        return {
            /** Checks that the user is authenticated, then ensures that are requires assets are loaded */
            isAuthenticatedAndReady: function ($q, userService, $route, assetsService, appState) {
                var deferred = $q.defer();

                //don't need to check if we've redirected to login and we've already checked auth
                if (!$route.current.params.section
                    && ($route.current.params.check === false || $route.current.params.check === "false")) {
                    deferred.resolve(true);
                    return deferred.promise;
                }
                
                userService.isAuthenticated()
                    .then(function () {

                        assetsService._loadInitAssets().then(function() {
                            
                            //This could be the first time has loaded after the user has logged in, in this case
                            // we need to broadcast the authenticated event - this will be handled by the startup (init)
                            // handler to set/broadcast the ready state
                            if (appState.getGlobalState("isReady") !== true) {
                                userService.getCurrentUser({ broadcastEvent: true }).then(function(user) {
                                    //is auth, check if we allow or reject
                                    if (isRequired) {
                                        //this will resolve successfully so the route will continue
                                        deferred.resolve(true);
                                    }
                                    else {
                                        deferred.reject({ path: "/" });
                                    }
                                });
                            }
                            else {
                                //is auth, check if we allow or reject
                                if (isRequired) {
                                    //this will resolve successfully so the route will continue
                                    deferred.resolve(true);
                                }
                                else {
                                    deferred.reject({ path: "/" });
                                }
                            }

                        });

                    }, function () {
                        //not auth, check if we allow or reject
                        if (isRequired) {
                            //the check=false is checked above so that we don't have to make another http call to check
                            //if they are logged in since we already know they are not.
                            deferred.reject({ path: "/login/false" });
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

    /** When this is used to resolve it will attempt to log the current user out */
    var doLogout = function() {
        return {
            isLoggedOut: function ($q, userService) {
                var deferred = $q.defer();
                userService.logout().then(function () {
                    //success so continue
                    deferred.resolve(true);
                }, function() {
                    //logout failed somehow ? we'll reject with the login page i suppose
                    deferred.reject({ path: "/login/false" });
                });
                return deferred.promise;
            }
        }
    }

    $routeProvider
        .when('/login', {
            templateUrl: 'views/common/login.html',
            //ensure auth is *not* required so it will redirect to / 
            resolve: canRoute(false)
        })
        .when('/login/:check', {
            templateUrl: 'views/common/login.html',
            //ensure auth is *not* required so it will redirect to / 
            resolve: canRoute(false)
        })
         .when('/logout', {
             redirectTo: '/login/false',             
             resolve: doLogout()
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
            resolve: canRoute(true)
        })
        .when('/:section/framed/:url', {
            //This occurs when we need to launch some content in an iframe
            templateUrl: function (rp) {
                if (!rp.url)
                    throw "A framed resource must have a url route parameter";

                return 'views/common/legacy.html';
            },
            resolve: canRoute(true)
        })              
        .when('/:section/:tree/:method', {
            templateUrl: function (rp) {
                if (!rp.method)
                    return "views/common/dashboard.html";

                //NOTE: This current isn't utilized by anything but does open up some cool opportunities for
                // us since we'll be able to have specialized views for individual sections which is something
                // we've never had before. So could utilize this for a new dashboard model when we get native
                // angular dashboards working. Perhaps a normal section dashboard would list out the registered
                // dashboards (as tabs if we wanted) and each tab could actually be a route link to one of these views?

                return 'views/' + rp.tree + '/' + rp.method + '.html';
            },
            resolve: canRoute(true)
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
                // /App_Plugins/{mypackage}/backoffice/{treetype}/{method}.html
                
                // otherwise if it is a core tree we use the core paths:
                // views/{treetype}/{method}.html

                var packageTreeFolder = treeService.getTreePackageFolder($routeParams.tree);

                if (packageTreeFolder) {
                    $scope.templateUrl = Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath +
                        "/" + packageTreeFolder +
                        "/backoffice/" + $routeParams.tree + "/" + $routeParams.method + ".html";
                }
                else {
                    $scope.templateUrl = 'views/' + $routeParams.tree + '/' + $routeParams.method + '.html';
                }
                
            },            
            resolve: canRoute(true)
        })        
        .otherwise({ redirectTo: '/login' });
    }).config(function ($locationProvider) {

        //$locationProvider.html5Mode(false).hashPrefix('!'); //turn html5 mode off
        // $locationProvider.html5Mode(true);         //turn html5 mode on
});