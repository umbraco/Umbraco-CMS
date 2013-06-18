angular.module('umbraco.services')
.factory('userService', function (authResource, $q) {

    var currentUser = null;    

    return {
        
        /** Returns a promise, sends a request to the server to check if the current cookie is authorized  */
        isAuthenticated: function() {
            var deferred = $q.defer();

            $q.when(authResource.isAuthenticated())
                .then(function(data) {
                    currentUser = data;
                    //note, this can return null if they are not authenticated
                    deferred.resolve({ user: data, authenticated: data == null ? false : true });
                },
                    function(reason) {
                        deferred.reject(reason);
                    });

            return deferred.promise;
        },        

        /** Returns a promise, sends a request to the server to validate the credentials  */
        authenticate: function (login, password) {

            var deferred = $q.defer();

            $q.when(authResource.performLogin(login, password))
                .then(function(data) {
                    currentUser = data;
                    deferred.resolve({ user: data, authenticated: true });
                },
                    function(reason) {
                        deferred.reject(reason);
                    });

            return deferred.promise;
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
