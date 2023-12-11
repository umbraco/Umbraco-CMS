(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbLogin', {
            templateUrl: 'views/components/application/umb-login.html',
            controller: UmbLoginController,
            controllerAs: 'vm',
            bindings: {
                isTimedOut: "<",
                onLogin: "&"
            }
        });

    function UmbLoginController($location, externalLoginInfoService, userService) {

        const vm = this;
        vm.onLoginSuccess = loginSuccess;

        vm.backgroundImage = Umbraco.Sys.ServerVariables.umbracoSettings.loginBackgroundImage;
        vm.logoImage = Umbraco.Sys.ServerVariables.umbracoSettings.loginLogoImage;
        vm.logoImageAlternative = Umbraco.Sys.ServerVariables.umbracoSettings.loginLogoImageAlternative;
        vm.allowPasswordReset = Umbraco.Sys.ServerVariables.umbracoSettings.canSendRequiredEmail && Umbraco.Sys.ServerVariables.umbracoSettings.allowPasswordReset;
        vm.usernameIsEmail = Umbraco.Sys.ServerVariables.umbracoSettings.usernameIsEmail;

        const tempUrl = new URL(Umbraco.Sys.ServerVariables.umbracoUrls.externalLoginsUrl, window.location.origin);
        tempUrl.searchParams.append("redirectUrl", decodeURIComponent($location.search().returnPath ?? ""))

        vm.externalLoginFormAction = tempUrl.pathname + tempUrl.search;
        vm.externalLoginProviders = externalLoginInfoService.getLoginProviders();
        vm.externalLoginProviders.forEach(x => {
            x.customView = externalLoginInfoService.getLoginProviderView(x);
        });
        vm.disableLocalLogin = externalLoginInfoService.hasDenyLocalLogin();

        /**
         * This is called when the user has successfully logged in
         * by the login screen sending out the event "umb-login-success"
         * @access private
         */
        function loginSuccess(evt) {
            const user = evt?.originalEvent?.detail;

            if (user) {
                userService.setAuthenticationSuccessful(user);
            } else {
                console.error("No user was returned from the login event");
            }

            if (vm.onLogin) {
                vm.onLogin();
            }
        }
    }

})();
