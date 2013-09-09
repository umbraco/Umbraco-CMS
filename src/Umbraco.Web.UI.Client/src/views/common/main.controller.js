
/**
 * @ngdoc controller
 * @name Umbraco.MainController
 * @function
 * 
 * @description
 * The main application controller
 * 
 */
function MainController($scope, $routeParams, $rootScope, $timeout, $http, $log, notificationsService, userService, navigationService, legacyJsLoader) {

    
    //detect if the current device is touch-enabled
    $scope.touchDevice = ("ontouchstart" in window || window.touch || window.navigator.msMaxTouchPoints===5 || window.DocumentTouch && document instanceof DocumentTouch);
    navigationService.touchDevice = $scope.touchDevice;

    //the null is important because we do an explicit bool check on this in the view
    //the avatar is by default the umbraco logo    
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
        var close = $(event.target).closest("#navigation");
        var parents = $(event.target).parents("#navigation");

        //SD: I've updated this so that we don't close the dialog when clicking inside of the dialog
        if (parents.length === 1) {
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


/*
            var url = "http://www.gravatar.com/avatar/" + $scope.user.emailHash + ".json?404=404";
            $http.jsonp(url).then(function(response){
                $log.log("found: " + response);
            }, function(data){
                $log.log(data);
            });
*/

            /*    
            if($scope.user.avatar){
                $http.get($scope.user.avatar).then(function(){
                    //alert($scope.user.avatar);
                    $scope.avatar = $scope.user.avatar;
                });
            }*/

            
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