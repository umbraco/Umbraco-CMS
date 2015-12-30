angular.module("umbraco").controller("Umbraco.Dialogs.LoginController",
    function ($scope, $cookies, localizationService, userService, externalLoginInfo) {

        /**
         * @ngdoc function
         * @name signin
         * @methodOf MainController
         * @function
         *
         * @description
         * signs the user in
         */



        var d = new Date();
        var konamiGreetings = new Array("Suze Sunday", "Malibu Monday", "Tequila Tuesday", "Whiskey Wednesday", "Negroni Day", "Fernet Friday", "Sancerre Saturday");
        var konamiMode = $cookies.konamiLogin;
        //var weekday = new Array("Super Sunday", "Manic Monday", "Tremendous Tuesday", "Wonderful Wednesday", "Thunder Thursday", "Friendly Friday", "Shiny Saturday");
        if (konamiMode == "1") {
            $scope.greeting = "Happy " + konamiGreetings[d.getDay()];
        } else {
            localizationService.localize("login_greeting" + d.getDay()).then(function (label) {
                $scope.greeting = label;
            }); // weekday[d.getDay()];
        }
        $scope.errorMsg = "";

        $scope.externalLoginFormAction = Umbraco.Sys.ServerVariables.umbracoUrls.externalLoginsUrl;
        $scope.externalLoginProviders = externalLoginInfo.providers;
        $scope.externalLoginInfo = externalLoginInfo;

        $scope.activateKonamiMode = function () {
            if ($cookies.konamiLogin == "1") {
                // somehow I can't update the cookie value using $cookies, so going native
                document.cookie = "konamiLogin=; expires=Thu, 01 Jan 1970 00:00:01 GMT;";
                document.location.reload();
            } else {
                document.cookie = "konamiLogin=1; expires=Tue, 01 Jan 2030 00:00:01 GMT;";
                $scope.$apply(function () {
                    $scope.greeting = "Happy " + konamiGreetings[d.getDay()];
                });
            }
        }

        $scope.loginSubmit = function (login, password) {

            //if the login and password are not empty we need to automatically 
            // validate them - this is because if there are validation errors on the server
            // then the user has to change both username & password to resubmit which isn't ideal,
            // so if they're not empty , we'l just make sure to set them to valid.
            if (login && password && login.length > 0 && password.length > 0) {
                $scope.loginForm.username.$setValidity('auth', true);
                $scope.loginForm.password.$setValidity('auth', true);
            }


            if ($scope.loginForm.$invalid) {
                return;
            }

            userService.authenticate(login, password)
                .then(function (data) {
                    $scope.submit(true);
                }, function (reason) {
                    $scope.errorMsg = reason.errorMsg;

                    //set the form inputs to invalid
                    $scope.loginForm.username.$setValidity("auth", false);
                    $scope.loginForm.password.$setValidity("auth", false);
                });

            //setup a watch for both of the model values changing, if they change
            // while the form is invalid, then revalidate them so that the form can 
            // be submitted again.
            $scope.loginForm.username.$viewChangeListeners.push(function () {
                if ($scope.loginForm.username.$invalid) {
                    $scope.loginForm.username.$setValidity('auth', true);
                }
            });
            $scope.loginForm.password.$viewChangeListeners.push(function () {
                if ($scope.loginForm.password.$invalid) {
                    $scope.loginForm.password.$setValidity('auth', true);
                }
            });
        };
    });
