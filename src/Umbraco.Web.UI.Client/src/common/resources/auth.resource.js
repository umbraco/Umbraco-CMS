/**
    * @ngdoc service 
    * @name umbraco.resources.authResource     
    * @description Loads in data for authentication
    **/
function authResource($q, $http, umbDataFormatter, umbRequestHelper) {

    /** internal method to get the api url */
    function getLoginUrl(username, password) {
        return Umbraco.Sys.ServerVariables.authenticationApiBaseUrl + "PostLogin?username=" + username + "&password=" + password;
    }
    
    /** internal method to get the api url */
    function getIsAuthUrl() {
        return Umbraco.Sys.ServerVariables.authenticationApiBaseUrl + "GetCurrentUser";
    }

    var _currentUser;
    

    return {
        currentUser: _currentUser,

        /** Logs the user in if the credentials are good */
        performLogin: function (username, password) {

            var deferred = $q.defer();
            //send the data
            $http.post(getLoginUrl(username, password)).
                success(function (data, status, headers, config) {
                    _currentUser = data;
                    deferred.resolve(data);                    
                }).
                error(function (data, status, headers, config) {
                    _currentUser = data;
                    deferred.reject('Login failed for user ' + username);
                });

            return deferred.promise;
        },
        
        /** Sends a request to the server to check if the current cookie value is valid for the user */
        isAuthenticated: function () {

            var deferred = $q.defer();
            //send the data
            $http.get(getIsAuthUrl()).
                success(function (data, status, headers, config) {
                    deferred.resolve(data);
                }).
                error(function (data, status, headers, config) {
                    if (status === 401) {
                        //if it's unauthorized it just means we are not authenticated so we'll just return null
                        deferred.resolve(null);
                    }
                    else {
                        deferred.reject('Server call failed for checking authorization');
                    }                    
                });

            return deferred.promise;
        }
    };
}

angular.module('umbraco.resources').factory('authResource', authResource);
