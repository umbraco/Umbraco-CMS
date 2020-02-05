angular.module('umbraco.services')
    .factory('userService', function ($rootScope, eventsService, $q, $location, requestRetryQueue, authResource, emailMarketingResource, $timeout, angularHelper) {

        var currentUser = null;
        var lastUserId = null;

        //this tracks the last date/time that the user's remainingAuthSeconds was updated from the server
        // this is used so that we know when to go and get the user's remaining seconds directly.
        var lastServerTimeoutSet = null;

        function openLoginDialog(isTimedOut) {
            //broadcast a global event that the user is no longer logged in
            const args = { isTimedOut: isTimedOut };
            eventsService.emit("app.notAuthenticated", args);
        }

        function retryRequestQueue(success) {
            if (success) {
                requestRetryQueue.retryAll(currentUser.name);
            }
            else {
                requestRetryQueue.cancelAll();
                $location.path('/');
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
            lastServerTimeoutSet = new Date();
            //start the timer
            countdownUserTimeout();
        }

        /**
        Method to count down the current user's timeout seconds,
        this will continually count down their current remaining seconds every 5 seconds until
        there are no more seconds remaining.
        */
        function countdownUserTimeout() {

            $timeout(function () {

                if (currentUser) {
                    //countdown by 5 seconds since that is how long our timer is for.
                    currentUser.remainingAuthSeconds -= 5;

                    //if there are more than 30 remaining seconds, recurse!
                    if (currentUser.remainingAuthSeconds > 30) {

                        //we need to check when the last time the timeout was set from the server, if
                        // it has been more than 30 seconds then we'll manually go and retrieve it from the
                        // server - this helps to keep our local countdown in check with the true timeout.
                        if (lastServerTimeoutSet != null) {
                            var now = new Date();
                            var seconds = (now.getTime() - lastServerTimeoutSet.getTime()) / 1000;

                            if (seconds > 30) {

                                //first we'll set the lastServerTimeoutSet to null - this is so we don't get back in to this loop while we
                                // wait for a response from the server otherwise we'll be making double/triple/etc... calls while we wait.
                                lastServerTimeoutSet = null;

                                //now go get it from the server
                                //NOTE: the safeApply because our timeout is set to not run digests (performance reasons)
                                angularHelper.safeApply($rootScope, function () {
                                    authResource.getRemainingTimeoutSeconds().then(function (result) {
                                        setUserTimeoutInternal(result);
                                    });
                                });
                            }
                        }

                        //recurse the countdown!
                        countdownUserTimeout();
                    }
                    else {

                        //we are either timed out or very close to timing out so we need to show the login dialog.
                        if (Umbraco.Sys.ServerVariables.umbracoSettings.keepUserLoggedIn !== true) {
                            //NOTE: the safeApply because our timeout is set to not run digests (performance reasons)
                            angularHelper.safeApply($rootScope, function () {
                                try {
                                    //NOTE: We are calling this again so that the server can create a log that the timeout has expired, we
                                    // don't actually care about this result.
                                    authResource.getRemainingTimeoutSeconds();
                                }
                                finally {
                                    userAuthExpired();
                                }
                            });
                        }
                        else {
                            //we've got less than 30 seconds remaining so let's check the server

                            if (lastServerTimeoutSet != null) {
                                //first we'll set the lastServerTimeoutSet to null - this is so we don't get back in to this loop while we
                                // wait for a response from the server otherwise we'll be making double/triple/etc... calls while we wait.
                                lastServerTimeoutSet = null;

                                //now go get it from the server
                                //NOTE: the safeApply because our timeout is set to not run digests (performance reasons)
                                angularHelper.safeApply($rootScope, function () {
                                    authResource.getRemainingTimeoutSeconds().then(function (result) {
                                        setUserTimeoutInternal(result);
                                    });
                                });
                            }

                            //recurse the countdown!
                            countdownUserTimeout();

                        }
                    }
                }
            }, 5000, //every 5 seconds
                false); //false = do NOT execute a digest for every iteration
        }

        /** Called to update the current user's timeout */
        function setUserTimeoutInternal(newTimeout) {

            var asNumber = parseFloat(newTimeout);
            if (!isNaN(asNumber) && currentUser && angular.isNumber(asNumber)) {
                currentUser.remainingAuthSeconds = newTimeout;
                lastServerTimeoutSet = new Date();
            }
        }

        /** resets all user data, broadcasts the notAuthenticated event and shows the login dialog */
        function userAuthExpired(isLogout) {
            //store the last user id and clear the user
            if (currentUser && currentUser.id !== undefined) {
                lastUserId = currentUser.id;
            }

            if (currentUser) {
                currentUser.remainingAuthSeconds = 0;
            }

            lastServerTimeoutSet = null;
            currentUser = null;

            openLoginDialog(isLogout === undefined ? true : !isLogout);
        }

        // Register a handler for when an item is added to the retry queue
        requestRetryQueue.onItemAddedCallbacks.push(function (retryItem) {
            if (requestRetryQueue.hasMore()) {
                userAuthExpired();
            }
        });

        return {

            /** Internal method to display the login dialog */
            _showLoginDialog: function () {
                openLoginDialog();
            },

            /** Internal method to retry all request after sucessfull login */
            _retryRequestQueue: function(success) {
                retryRequestQueue(success)
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
                    .then(this.setAuthenticationSuccessful);
            },
            setAuthenticationSuccessful: function (data) {

                //when it's successful, return the user data
                setCurrentUser(data);

                var result = { user: data, authenticated: true, lastUserId: lastUserId, loginType: "credentials" };

                //broadcast a global event
                eventsService.emit("app.authenticated", result);
                return result;
            },

            /** Logs the user out
             */
            logout: function () {

                return authResource.performLogout()
                    .then(function (data) {
                        userAuthExpired();
                        //done!
                        return null;
                    });
            },

            /** Refreshes the current user data with the data stored for the user on the server and returns it */
            refreshCurrentUser: function () {
                var deferred = $q.defer();

                authResource.getCurrentUser()
                    .then(function (data) {

                        var result = { user: data, authenticated: true, lastUserId: lastUserId, loginType: "implicit" };

                        setCurrentUser(data);

                        deferred.resolve(currentUser);
                    }, function () {
                        //it failed, so they are not logged in
                        deferred.reject();
                    });

                return deferred.promise;
            },

            /** Returns the current user object in a promise  */
            getCurrentUser: function (args) {
                
                if (!currentUser) {
                    return authResource.getCurrentUser()
                        .then(function (data) {

                            var result = { user: data, authenticated: true, lastUserId: lastUserId, loginType: "implicit" };

                            if (args && args.broadcastEvent) {
                                //broadcast a global event, will inform listening controllers to load in the user specific data
                                eventsService.emit("app.authenticated", result);
                            }

                            setCurrentUser(data);

                            return $q.when(currentUser);
                        }, function () {
                            //it failed, so they are not logged in
                            return $q.reject(currentUser);
                        });

                }
                else {
                    return $q.when(currentUser);
                }
            },

            /** Called whenever a server request is made that contains a x-umb-user-seconds response header for which we can update the user's remaining timeout seconds */
            setUserTimeout: function (newTimeout) {
                setUserTimeoutInternal(newTimeout);
            },

            /** Calls out to a Remote Azure Function to deal with email marketing service */
            addUserToEmailMarketing: (user) => {
                return emailMarketingResource.postAddUserToEmailMarketing(user);
            }
        };

    });
