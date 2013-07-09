
/**
 * @ngdoc controller
 * @name MainController
 * @function
 * 
 * @description
 * The main application controller
 * 
 */
function MainController($scope, $routeParams, $rootScope, $timeout, notificationsService, userService, navigationService, legacyJsLoader) {
    //set default properties
    $scope.authenticated = null; //the null is important because we do an explicit bool check on this in the view    
    
    //subscribes to notifications in the notification service
    $scope.notifications = notificationsService.current;
    $scope.$watch('notificationsService.current', function (newVal, oldVal, scope) {
        if (newVal) {
            $scope.notifications = newVal;
        }
    });

    $scope.removeNotification = function (index) {
        notificationsService.remove(index);
    };

    $scope.closeDialogs = function (event) {
        //only close dialogs if non-lin and non-buttons are clicked
        var el = event.target.nodeName;
        var pEl = event.target.parentElement.nodeName;

        if(el != "A" && el != "BUTTON" && pEl != "A" && pEl != "BUTTON"){
            $rootScope.$emit("closeDialogs");
        }

        if (navigationService.ui.stickyNavigation && $(event.target).parents(".umb-modalcolumn").size() == 0) {
            navigationService.hideNavigation();
        }
    };

    //fetch the authorized status         
    userService.isAuthenticated()
        .then(function (data) {
            
            //We need to load in the legacy tree js.
            legacyJsLoader.loadLegacyTreeJs($scope).then(
                function (result) {
                    //TODO: We could wait for this to load before running the UI ?
                });
            
            $scope.authenticated = data.authenticated;
            $scope.user = data.user;
            
        }, function (reason) {
            notificationsService.error("An error occurred checking authentication.");
            $scope.authenticated = false;
            $scope.user = null;
        });
}

//register it
angular.module('umbraco').controller("MainController", MainController);
