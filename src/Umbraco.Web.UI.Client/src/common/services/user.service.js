angular.module('umbraco.services')
.factory('userService', function (authResource, $q) {

    var currentUser = null;

    return {

        /** Returns a promise, sends a request to the server to check if the current cookie is authorized  */
        isAuthenticated: function () {
            
            return $q.when(authResource.isAuthenticated())
                .then(function(data) {

                    //note, this can return null if they are not authenticated
                    if (!data) {
                        throw "Not authenticated";
                    }
                    else {
                        currentUser = data;
                        currentUser.avatar = 'http://www.gravatar.com/avatar/' + data.emailHash + '?s=40';
                        return { user: data, authenticated: true };
                    }
                });
        },

        /** Returns a promise, sends a request to the server to validate the credentials  */
        authenticate: function (login, password) {

            return $q.when(authResource.performLogin(login, password))
                .then(function (data) {
                    //when it's successful, return the user data
                    currentUser = data;
                    return { user: data, authenticated: true };
                });
        },

        logout: function () {
            $rootScope.$apply(function () {
                currentUser = undefined;
            });
        },

        /** Returns the current user object, if null then calls to authenticated or authenticate must be called  */
        getCurrentUser: function () {
            return currentUser;
        }
    };

});
