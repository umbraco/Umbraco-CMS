
/**
 * @ngdoc controller
 * @name MainController
 * @function
 * 
 * @description
 * The main application controller
 * 
 */
function MainController($scope, $routeParams, $rootScope, $timeout, notificationsService, userService, navigationService) {
    //also be authed for e2e test
    var d = new Date();
    var weekday = new Array("Super Sunday", "Manic Monday", "Tremendous Tuesday", "Wonderfull Wednesday", "Thunder Thursday", "Friendly Friday", "Shiny Saturday");
    $scope.today = weekday[d.getDay()];


    //set default properties
    $scope.authenticated = null; //the null is important because we do an explicit bool check on this in the view    
    $scope.login = "";
    $scope.password = "";


    $scope.signout = function () {
        userService.signout();
        $scope.authenticated = false;
    };

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
        $rootScope.$emit("closeDialogs");

        if (navigationService.ui.stickyNavigation && $(event.target).parents(".umb-modalcolumn").size() == 0) {
            navigationService.hideNavigation();
        }
    };

    //fetch the authorized status         
    userService.isAuthenticated()
        .then(function (data) {
            $scope.authenticated = data.authenticated;
            $scope.user = data.user;
        }, function (reason) {
            alert("An error occurred checking authentication.");
            $scope.authenticated = false;
            $scope.user = null;
        });
}

//register it
angular.module('umbraco').controller("MainController", MainController);