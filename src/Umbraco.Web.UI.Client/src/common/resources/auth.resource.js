/**
    * @ngdoc service
    * @name umbraco.resources.authResource
    * @description Loads in data for authentication
**/
function authResource($q, $http, umbRequestHelper, angularHelper) {

    return {
        //currentUser: currentUser,

        /** Logs the user in if the credentials are good */
        performLogin: function (username, password) {
            
            if (!username || !password) {
                return angularHelper.rejectedPromise({
                    errorMsg: 'Username or password cannot be empty'
                });
            }

            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "PostLogin"), {
                            username: username,
                            password: password
                        }),
                'Login failed for user ' + username);
        },
        
        performLogout: function() {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "PostLogout")));
        },
        
        /** Sends a request to the server to get the current user details, will return a 401 if the user is not logged in  */
        getCurrentUser: function () {
            
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "GetCurrentUser")),
                'Server call failed for getting current user'); 
        },
        
        /** Checks if the user is logged in or not - does not return 401 or 403 */
        isAuthenticated: function () {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "IsAuthenticated")),
                'Server call failed for checking authentication');
        },
        
        /** Gets the user's remaining seconds before their login times out */
        getRemainingTimeoutSeconds: function () {

            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "authenticationApiBaseUrl",
                        "GetRemainingTimeoutSeconds")),
                'Server call failed for checking remaining seconds');
        }

    };
}

angular.module('umbraco.resources').factory('authResource', authResource);
