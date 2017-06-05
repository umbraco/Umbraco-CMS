angular.module("umbraco.services").factory("twoFactorService", function ($http) {
    return {
        getEnabled: function (userId) {
            return $http.get("/umbraco/backoffice/UmbracoApi/TwoFactorAuthentication/TwoFactorEnabled/?userId=" + userId);
        },
        getGoogleAuthenticatorSetupCode: function () {
            return $http.get("/umbraco/backoffice/UmbracoApi/TwoFactorAuthentication/GoogleAuthenticatorSetupCode/");
        },
        validateAndSave: function (code) {
            return $http.post("/umbraco/backoffice/UmbracoApi/TwoFactorAuthentication/ValidateAndSave/?code=" + code);
        },
        validateAndSaveGoogleAuth: function (code) {
            return $http.post("/umbraco/backoffice/UmbracoApi/TwoFactorAuthentication/ValidateAndSaveGoogleAuth/?code=" + code);
        },
        disable: function () {
            return $http.post("/umbraco/backoffice/UmbracoApi/TwoFactorAuthentication/Disable/");
        }
    };
});