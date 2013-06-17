angular.module('umbraco.services')
.factory('userService', function (authResource, $q) {

    var currentUser;
    var authenticated = (jQuery.cookie('UMB_UCONTEXT') != null && jQuery.cookie('UMB_UCONTEXT') != "");

    //var _mockedU = {
    //  name: "Per Ploug", 
    //  avatar: "assets/img/avatar.jpeg", 
    //  id: 0,
    //  authenticated: true,
    //  locale: 'da-DK' 
    //};

    //if(_authenticated){
    //  _currentUser = _mockedU; 
    //}

    return {
        authenticated: authenticated,        

        authenticate: function (login, password) {

            var deferred = $q.defer();

            authResource.performLogin(login, password)
                .then(function (data) {
                    currentUser = data;
                    authenticated = true;
                    deferred.resolve({ user: data, authenticated: true });
                },
                function (reason) {
                    deferred.reject(reason);
                    authenticated = false;
                });

            return deferred.promise;
        },

        logout: function () {
            $rootScope.$apply(function () {
                authenticated = false;
                jQuery.cookie('authed', null);
                currentUser = undefined;
            });
        },

        getCurrentUser: function () {
            return currentUser;
        }
    };

});
