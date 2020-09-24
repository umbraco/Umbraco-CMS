app.config(function ($routeProvider) {
    
    /**
     * This determines if the route can continue depending on authentication and initialization requirements
     * @param {boolean} authRequired If true, it checks if the user is authenticated and will resolve successfully
        otherwise the route will fail and the $routeChangeError event will execute, in that handler we will redirect to the rejected
        path that is resolved from this method and prevent default (prevent the route from executing)
     * @returns {promise} 
     */
    var canRoute = function(authRequired) {

        return {
            /** Checks that the user is authenticated, then ensures that are requires assets are loaded */
            isAuthenticatedAndReady: function ($q, userService, $route, assetsService, appState) {

                //don't need to check if we've redirected to login and we've already checked auth
                if (!$route.current.params.section
                    && ($route.current.params.check === false || $route.current.params.check === "false")) {
                    return $q.when(true);
                }

                return userService.isAuthenticated()
                    .then(function () {

                        //before proceeding all initial assets must be loaded
                        return assetsService._loadInitAssets().then(function () {

                            //This could be the first time has loaded after the user has logged in, in this case
                            // we need to broadcast the authenticated event - this will be handled by the startup (init)
                            // handler to set/broadcast the ready state
                            var broadcast = appState.getGlobalState("isReady") !== true;

                            return userService.getCurrentUser({ broadcastEvent: broadcast }).then(function (user) {
                                //is auth, check if we allow or reject
                                if (authRequired) {

                                    //This checks the current section and will force a redirect to 'content' as the default
                                    if ($route.current.params.section.toLowerCase() === "default" || $route.current.params.section.toLowerCase() === "umbraco" || $route.current.params.section === "") {
                                        $route.current.params.section = "content";
                                    }

                                    var found = _.find(user.allowedSections, function (s) {
                                        return s.localeCompare($route.current.params.section, undefined, { sensitivity: 'accent' }) === 0;
                                    })

                                    // U4-5430, Benjamin Howarth
                                    // We need to change the current route params if the user only has access to a single section
                                    // To do this we need to grab the current user's allowed sections, then reject the promise with the correct path.
                                    if (found) {
                                        //this will resolve successfully so the route will continue
                                        return $q.when(true);
                                    } else {
                                        return $q.reject({ path: "/" + user.allowedSections[0] });
                                    }
                                }
                                else {
                                    return $q.when(true);
                                }
                            });

                        });

                    }, function () {
                        //not auth, check if we allow or reject
                        if (authRequired) {
                            //the check=false is checked above so that we don't have to make another http call to check
                            //if they are logged in since we already know they are not.
                            return $q.reject({ path: "/login/false" });
                        }
                        else {
                            //this will resolve successfully so the route will continue
                            return $q.when(true);
                        }
                    });

            }
        };
    };

    /** When this is used to resolve it will attempt to log the current user out */
    var doLogout = function() {
        return {
            isLoggedOut: function ($q, $location, userService) {
                return userService.logout().then(function () {
                    // we have to redirect here instead of the routes redirectTo
                    // https://github.com/angular/angular.js/commit/7f4b356c2bebb87f0c26b57a20415b004b20bcd1
                    $location.path("/login/false");
                    //success so continue
                    return $q.when(true);
                }, function() {
                    //logout failed somehow ? we'll reject with the login page i suppose
                    return $q.reject({ path: "/login/false" });
                });
            }
        }
    }

    $routeProvider
        .when("/", {
            redirectTo: '/content'
        })
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
            resolve: doLogout()
        })
        .when('/:section?', {
            //This allows us to dynamically change the template for this route since you cannot inject services into the templateUrl method.
            template: "<div ng-include='templateUrl'></div>",
            //This controller will execute for this route, then we can execute some code in order to set the template Url
            controller: function ($scope, $route, $routeParams, $location, sectionService) {
                
                //We are going to check the currently loaded sections for the user and if the section we are navigating
                //to has a custom route path we'll use that 
                sectionService.getSectionsForUser().then(function(sections) {
                    //find the one we're requesting
                    var found = _.find(sections, function(s) {
                        return s.alias.localeCompare($routeParams.section, undefined, { sensitivity: 'accent' }) === 0;
                    })
                    if (found && found.routePath) {
                        //there's a custom route path so redirect
                        $location.path(found.routePath);
                    }
                    else {
                        //there's no custom route path so continue as normal
                        $routeParams.url = "dashboard.aspx?app=" + $routeParams.section;
                        $scope.templateUrl = 'views/common/dashboard.html';
                    }
                });
            },
            reloadOnSearch: false,
            resolve: canRoute(true)
        })
        .when('/:section/framed/:url', {
            //This occurs when we need to launch some content in an iframe
            templateUrl: function (rp) {
                if (!rp.url)
                    throw "A framed resource must have a url route parameter";

                return 'views/common/legacy.html';
            },
            reloadOnSearch: false,
            resolve: canRoute(true)
        })
        .when('/:section/:tree/:method?', {
            //This allows us to dynamically change the template for this route since you cannot inject services into the templateUrl method.
            template: "<div ng-include='templateUrl'></div>",
            //This controller will execute for this route, then we replace the template dynamically based on the current tree.
            controller: function ($scope, $routeParams, navigationService) {

                if (!$routeParams.method) {
                    $scope.templateUrl = "views/common/dashboard.html";
                    return;
                }

                //TODO: Fix this special case by using components, the packager should be a component and then we just have a view for each route like normal rendering the component with the correct parameters
                //special case for the package section
                var packagePages = ["edit", "options"];
                if ($routeParams.section.toLowerCase() === "packages" && $routeParams.tree.toLowerCase() === "packages" && packagePages.indexOf($routeParams.method.toLowerCase()) === -1) {
                    $scope.templateUrl = "views/packages/overview.html";
                    return;
                }

                //TODO: Fix this special case by using components, the users section should be a component and then we just have a view for each route like normal rendering the component with the correct parameters
                //special case for the users section
                var usersPages = ["user", "group"];
                if ($routeParams.section.toLowerCase() === "users" && $routeParams.tree.toLowerCase() === "users" && usersPages.indexOf($routeParams.method.toLowerCase()) === -1) {
                    $scope.templateUrl = "views/users/overview.html";
                    return;
                }
                $scope.templateUrl = navigationService.getTreeTemplateUrl($routeParams.tree, $routeParams.method);
            },
            reloadOnSearch: false,
            resolve: canRoute(true)
        })
        .when('/:section/:tree/:method?/:id', {
            //This allows us to dynamically change the template for this route since you cannot inject services into the templateUrl method.
            template: "<div ng-include='templateUrl'></div>",
            //This controller will execute for this route, then we replace the template dynamically based on the current tree.
            controller: function ($scope, $routeParams, navigationService) {

                if (!$routeParams.tree || !$routeParams.method) {
                    $scope.templateUrl = "views/common/dashboard.html";
                    return;
                }
                $scope.templateUrl = navigationService.getTreeTemplateUrl($routeParams.tree, $routeParams.method);
            },
            reloadOnSearch: false,
            reloadOnUrl: false,
            resolve: canRoute(true)
        })
        .otherwise({ redirectTo: '/login' });
    }).config(function ($locationProvider) {
        
        $locationProvider.html5Mode(false); //turn html5 mode off
        $locationProvider.hashPrefix('');
    });
