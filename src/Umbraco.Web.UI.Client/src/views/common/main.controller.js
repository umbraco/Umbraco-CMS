
/**
 * @ngdoc controller
 * @name Umbraco.MainController
 * @function
 * 
 * @description
 * The main application controller
 * 
 */
function MainController($scope, $routeParams, $rootScope, $timeout, $http, notificationsService, userService, navigationService, legacyJsLoader) {
    //debugmode so I can easily turn on/off json output of property models:
    //TODO: find a better way
    $scope.$umbdebugmode = true;

    //set default properties
    
    //the null is important because we do an explicit bool check on this in the view    
    $scope.authenticated = null;
    $scope.avatar = "assets/img/application/logo.png";

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

        //SD: I've updated this so that we don't close the dialog when clicking inside of the dialog
        if ($(event.target).closest("#navigation").length === 1) {
            return;
        }

        //SD: I've added a check for INPUT elements too
        if(el != "I" && el != "A" && el != "BUTTON" && pEl != "A" && pEl != "BUTTON" && el != "INPUT" && pEl != "INPUT"){
            $rootScope.$emit("closeDialogs", event);
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


            if($scope.user.avatar){
                $http.get($scope.user.avatar).then(function(){
                    //alert($scope.user.avatar);
                    $scope.avatar = $scope.user.avatar;
                });
            }

            
        }, function (reason) {
            notificationsService.error("An error occurred checking authentication.");
            $scope.authenticated = false;
            $scope.user = null;
        });
}


//register it
angular.module('umbraco').controller("Umbraco.MainController", MainController);

/*
angular.module("umbraco").run(function(eventsService){
    eventsService.subscribe("Umbraco.Dialogs.ContentPickerController.Select", function(a, b){
        a.node.name = "wat";
    });
});
*/