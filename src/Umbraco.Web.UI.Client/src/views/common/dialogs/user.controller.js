angular.module("umbraco")
    .controller("Umbraco.Dialogs.UserController", function ($scope, $location, userService, historyService) {
       
        $scope.user = userService.getCurrentUser();
        $scope.history = historyService.current;

        $scope.logout = function () {
	        userService.logout();
	        $scope.hide();
	        $location.path("/");
    	};

	    $scope.gotoHistory = function (link) {
		    $location.path(link);
		    $scope.$apply() 
		    $scope.hide();
		};
});