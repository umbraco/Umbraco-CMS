// Based loosely around work by Witold Szczerba - https://github.com/witoldsz/angular-http-auth

//TODO: We need to streamline the usage of this class and the overlap it has with userService and authResource!
// SD: IMO, I think we can remove this class and just put this all into the userService class.

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
              queue.retryAll();
          } else {
              queue.cancelAll();
              redirect();
          }
      }
      
      // The public API of the service
      var service = {
      
          // Show the modal login dialog
          showLogin: function () {
              openLoginDialog();
          },

          // Ask the backend to see if a user is already authenticated - this may be from a previous session.
          requestCurrentUser: function () {
              if (service.isAuthenticated()) {
                  return $q.when(service.currentUser);
              } else {
                  service.currentUser = authResource.currentUser;
                  return service.currentUser;
              }
          },

          // Information about the current user
          currentUser: null,

          // Is the current user authenticated?
          isAuthenticated: function () {
              return !!service.currentUser;
          }
      };

      return service;
  }]);