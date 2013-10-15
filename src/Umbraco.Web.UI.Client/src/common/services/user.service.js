angular.module('umbraco.services')
.factory('userService', function ($rootScope, $q, $location, $log, securityRetryQueue, authResource, dialogService, $timeout) {

    var currentUser = null;
    var lastUserId = null;
    var loginDialog = null;

    // Redirect to the given url (defaults to '/')
    function redirect(url) {
        url = url || '/';
        $location.path(url);
    }

    function openLoginDialog() {
        if (!loginDialog) {
            loginDialog = dialogService.open({
                template: 'views/common/dialogs/login.html',
                modalClass: "login-overlay",
                animation: "slide",
                show: true,
                callback: onLoginDialogClose
            });
        }
    }

    function onLoginDialogClose(success) {
        loginDialog = null;

        if (success) {
            securityRetryQueue.retryAll();
        } else {
            securityRetryQueue.cancelAll();
            redirect();
        }
    }

    /** 
    This methods will set the current user when it is resolved and 
    will then start the counter to count in-memory how many seconds they have 
    remaining on the auth session
    */
    function setCurrentUser(usr) {
        if (!usr.remainingAuthSeconds) {
            throw "The user object is invalid, the remainingAuthSeconds is required.";
        }
        currentUser = usr;
        //start the timer
        setCurrentUserTimeout(usr);
    }
    function setCurrentUserTimeout() {        
        $timeout(function () {
            if (currentUser) {
                currentUser.remainingAuthSeconds -= 1;
                if (currentUser.remainingAuthSeconds > 0) {
                    //recurse!
                    setCurrentUserTimeout();
                }
            }            
        }, 1000);//every second
    }

    // Register a handler for when an item is added to the retry queue
    securityRetryQueue.onItemAddedCallbacks.push(function (retryItem) {
        if (securityRetryQueue.hasMore()) {
            
            //store the last user id and clear the user
            if (currentUser && currentUser.id !== undefined) {
                lastUserId = currentUser.id;
            }
            currentUser = null;

            //broadcast a global event that the user is no longer logged in
            $rootScope.$broadcast("notAuthenticated");

            openLoginDialog();
        }
    });

    return {

        /** Internal method to display the login dialog */
        _showLoginDialog: function () {
            openLoginDialog();
        },

        /** Returns a promise, sends a request to the server to check if the current cookie is authorized  */
        isAuthenticated: function () {
            //if we've got a current user then just return true
            if (currentUser) {
                var deferred = $q.defer();
                deferred.resolve(true);
                return deferred.promise;
            }
            return authResource.isAuthenticated();
        },

        /** Returns a promise, sends a request to the server to validate the credentials  */
        authenticate: function (login, password) {

            return authResource.performLogin(login, password)
                .then(function (data) {

                    //when it's successful, return the user data
                    setCurrentUser(data);

                    var result = { user: data, authenticated: true, lastUserId: lastUserId };

                    //broadcast a global event
                    $rootScope.$broadcast("authenticated", result);

                    return result;
                });
        },

        /** Logs the user out and redirects to the login page */
        logout: function () {
            return authResource.performLogout()
                .then(function (data) {                   

                    lastUserId = currentUser.id;
                    currentUser = null;

                    //broadcast a global event
                    $rootScope.$broadcast("notAuthenticated");

                    $location.path("/login").search({check: false});

                    return null;
                });
        },

        /** Returns the current user object in a promise  */
        getCurrentUser: function (args) {
            var deferred = $q.defer();
            
            if (!currentUser) {
                authResource.getCurrentUser()
                    .then(function(data) {

                        var result = { user: data, authenticated: true, lastUserId: lastUserId };

                        if (args.broadcastEvent) {
                            //broadcast a global event, will inform listening controllers to load in the user specific data
                            $rootScope.$broadcast("authenticated", result);
                        }

                        setCurrentUser(data);
                        currentUser.avatar = 'http://www.gravatar.com/avatar/' + data.emailHash + '?s=40&d=404';
                        deferred.resolve(currentUser);
                    });

            }
            else {
                deferred.resolve(currentUser);
            }
            
            return deferred.promise;
        },
        
        setUserTimeout: function(newTimeout) {
            if (currentUser && angular.isNumber(newTimeout)) {
                currentUser.remainingAuthSeconds = newTimeout;
            }
        }
    };

});
