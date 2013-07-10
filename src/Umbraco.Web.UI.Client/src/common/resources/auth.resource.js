/**
    * @ngdoc service 
    * @name umbraco.resources.authResource     
    * @description Loads in data for authentication
    **/
function authResource($q, $http, angularHelper) {

    /** internal method to get the api url */
    function getLoginUrl(username, password) {
        return Umbraco.Sys.ServerVariables.authenticationApiBaseUrl + "PostLogin?username=" + username + "&password=" + password;
    }
    
    /** internal method to get the api url */
    function getIsAuthUrl() {
        return Umbraco.Sys.ServerVariables.authenticationApiBaseUrl + "GetCurrentUser";
    }

    //var currentUser;
    

    return {
        //currentUser: currentUser,

        /** Logs the user in if the credentials are good */
        performLogin: function (username, password) {
            return angularHelper.resourcePromise(
                $http.post(getLoginUrl(username, password)),
                'Login failed for user ' + username);
        },
        
        /** Sends a request to the server to check if the current cookie value is valid for the user */
        isAuthenticated: function () {
            
            return angularHelper.resourcePromise(
                $http.get(getIsAuthUrl()),
                'Server call failed for checking authorization');
        }
    };
}

angular.module('umbraco.resources').factory('authResource', authResource);
