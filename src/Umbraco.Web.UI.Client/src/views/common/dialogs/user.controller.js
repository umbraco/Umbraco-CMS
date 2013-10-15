angular.module("umbraco")
    .controller("Umbraco.Dialogs.UserController", function ($scope, $location, userService, historyService) {
       
        $scope.user = userService.getCurrentUser();
        $scope.history = historyService.current;

        $scope.logout = function () {
            userService.logout().then(function() {
                $scope.hide();                
            });
    	};

	    $scope.gotoHistory = function (link) {
		    $location.path(link);	        
		    $scope.hide();
		};
});