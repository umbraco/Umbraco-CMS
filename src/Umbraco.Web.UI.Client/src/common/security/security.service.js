// Based loosely around work by Witold Szczerba - https://github.com/witoldsz/angular-http-auth

angular.module('umbraco.security.service', [
  'umbraco.security.retryQueue',    // Keeps track of failed requests that need to be retried once the user logs in
  'umbraco.resources',
  'umbraco.services'
])
.factory('security', ['$http', '$q', '$location', 'securityRetryQueue', 'authResource', 'dialogService', '$log',
  function ($http, $q, $location, queue, authResource, dialogService, $log) {

  // Register a handler for when an item is added to the retry queue
  queue.onItemAddedCallbacks.push(function (retryItem) {
        if (queue.hasMore()) {
            service.showLogin();
        }
   });
    

  // Redirect to the given url (defaults to '/')
  function redirect(url) {
    url = url || '/';
    $location.path(url);
  }

  // Login form dialog stuff
  var loginDialog = null;

  function openLoginDialog() {
    if ( loginDialog ) {
      throw new Error('Trying to open a dialog that is already open!');
    }

    loginDialog = dialogService.open({
                      template: 'views/common/dialogs/login.html', 
                      modalClass: "login-overlay", 
                      show: true, 
                      callback: onLoginDialogClose});
  }
      
  function closeLoginDialog(success) {
    if (loginDialog) {
        loginDialog = null;
    }
  }

  function onLoginDialogClose(success) {
    loginDialog = null;
    if ( success ) {
      queue.retryAll();
    } else {
      queue.cancelAll();
      redirect();
    }
  }


  // The public API of the service
  var service = {

    // Get the first reason for needing a login
    getLoginReason: function() {
      return queue.retryReason();
    },

    // Show the modal login dialog
    showLogin: function() {
      openLoginDialog();
    },

    // Attempt to authenticate a user by the given email and password
    login: function(email, password) {
      var request = $http.post('/login', {email: email, password: password});
      return request.then(function(response) {
        service.currentUser = response.data.user;
        if ( service.isAuthenticated() ) {
          closeLoginDialog(true);
        }
      });
    },

    // Give up trying to login and clear the retry queue
    cancelLogin: function() {
      closeLoginDialog(false);
      redirect();
    },

    // Logout the current user and redirect
    logout: function(redirectTo) {
      $http.post('/logout').then(function() {
        service.currentUser = null;
        redirect(redirectTo);
      });
    },

    // Ask the backend to see if a user is already authenticated - this may be from a previous session.
    requestCurrentUser: function() {
      if ( service.isAuthenticated() ) {
        return $q.when(service.currentUser);
      } else {
        service.currentUser = authResource.currentUser;
        return service.currentUser;
      }
    },

    // Information about the current user
    currentUser: null,

    // Is the current user authenticated?
    isAuthenticated: function(){
      return !!service.currentUser;
    },
    
    // Is the current user an adminstrator?
    isAdmin: function() {
      return !!(service.currentUser && service.currentUser.admin);
    }
  };

  return service;
}]);