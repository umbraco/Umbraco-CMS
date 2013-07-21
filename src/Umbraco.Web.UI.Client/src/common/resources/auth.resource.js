/**
* @ngdoc service 
* @name umbraco.resources.authResource     
* @description Loads in data for authentication
**/
function authResource($q, $http, umbRequestHelper) {

    return {
        //currentUser: currentUser,

        /** Logs the user in if the credentials are good */
        performLogin: function (username, password) {

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "PostLogin",
                        [{ username: username }, { password: password }])),
                'Login failed for user ' + username);
        },
        
        /** Sends a request to the server to check if the current cookie value is valid for the user */
        isAuthenticated: function () {
            
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "GetCurrentUser")),
                'Server call failed for checking authorization'); 
        }
    };
}

angular.module('umbraco.resources').factory('authResource', authResource);
