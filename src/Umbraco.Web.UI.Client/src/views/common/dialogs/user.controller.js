angular.module("umbraco")
    .controller("Umbraco.Dialogs.UserController", function ($scope, $location, $timeout, userService, historyService) {
       
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

        //Manually update the remaining timeout seconds
        function updateTimeout() {
            $timeout(function () {
                $scope.user = userService.getCurrentUser();
                //manually execute the digest against this scope only
                $scope.$digest();
                updateTimeout(); //keep going (recurse)
            }, 1000, false); // 1 second, do NOT execute a global digest    
        }
        updateTimeout();
        
    });