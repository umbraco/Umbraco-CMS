/**
    * @ngdoc factory 
    * @name umbraco.resources.authResource     
    * @description Loads in data for authentication
    **/
function authResource($q, $http, umbDataFormatter, umbRequestHelper) {

    var mocked = {
        name: "Per Ploug",
        email: "test@test.com",
        emailHash: "f9879d71855b5ff21e4963273a886bfc",
        id: 0,
        locale: 'da-DK'
    };

    return {
        
        /** Logs the user in if the credentials are good */
        performLogin: function (username, password) {
            return mocked;
        },
        
        /** Sends a request to the server to check if the current cookie value is valid for the user */
        isAuthenticated: function () {
            return mocked;
        }
    };
}

angular.module('umbraco.mocks.resources').factory('authResource', authResource);
