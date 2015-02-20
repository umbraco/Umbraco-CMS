angular.module("umbraco").controller("Umbraco.Dialogs.LoginController",
    function ($scope, localizationService, userService, externalLoginInfo) {

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
        //var weekday = new Array("Super Sunday", "Manic Monday", "Tremendous Tuesday", "Wonderful Wednesday", "Thunder Thursday", "Friendly Friday", "Shiny Saturday");
        localizationService.localize("login_greeting" + d.getDay()).then(function (label) {
            $scope.greeting = label;
        }); // weekday[d.getDay()];

        $scope.errorMsg = "";

        $scope.externalLoginFormAction = Umbraco.Sys.ServerVariables.umbracoUrls.externalLoginsUrl;
        $scope.externalLoginProviders = externalLoginInfo.providers;
        $scope.externalLoginInfo = externalLoginInfo;

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
