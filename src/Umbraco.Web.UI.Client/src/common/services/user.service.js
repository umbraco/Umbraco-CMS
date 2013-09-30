angular.module('umbraco.services')
.factory('userService', function ($rootScope, $q, $location, $log, securityRetryQueue, authResource, dialogService) {

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

        /** Returns a promise, sends a request to the server to check if the current cookie is authorized  */
        isAuthenticated: function (args) {
            
            return authResource.isAuthenticated()
                .then(function(data) {

                    //note, this can return null if they are not authenticated
                    if (!data) {                        
                        throw "Not authenticated";
                    }
                    else {

                        var result = { user: data, authenticated: true, lastUserId: lastUserId };

                        if (args.broadcastEvent) {
                            //broadcast a global event, will inform listening controllers to load in the user specific data
                            $rootScope.$broadcast("authenticated", result);
                        }

                        currentUser = data;
                        currentUser.avatar = 'http://www.gravatar.com/avatar/' + data.emailHash + '?s=40&d=404';
                        return result;
                    }
                });
        },

        /** Returns a promise, sends a request to the server to validate the credentials  */
        authenticate: function (login, password) {

            return authResource.performLogin(login, password)
                .then(function (data) {

                    //when it's successful, return the user data
                    currentUser = data;

                    var result = { user: data, authenticated: true, lastUserId: lastUserId };

                    //broadcast a global event
                    $rootScope.$broadcast("authenticated", result);

                    return result;
                });
        },

        logout: function () {
            return authResource.performLogout()
                .then(function (data) {                   

                    lastUserId = currentUser.id;
                    currentUser = null;

                    //broadcast a global event
                    $rootScope.$broadcast("notAuthenticated");

                    openLoginDialog();
                    return null;
                });
        },

        /** Returns the current user object, if null then calls to authenticated or authenticate must be called  */
        getCurrentUser: function () {
            return currentUser;
        }
    };

});
