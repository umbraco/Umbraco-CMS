angular.module("umbraco")
    .controller("Umbraco.Dialogs.UserController", function ($scope, $location, $timeout, userService, historyService) {
       
        $scope.user = userService.getCurrentUser();
        $scope.history = historyService.current;
        $scope.version = Umbraco.Sys.ServerVariables.application.version + " assembly: " + Umbraco.Sys.ServerVariables.application.assemblyVersion;


        $scope.logout = function () {
            userService.logout().then(function () {
                $scope.remainingAuthSeconds = 0;
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
	            if ($scope.remainingAuthSeconds > 0) {
	                $scope.remainingAuthSeconds--;
	                $scope.$digest();
	                //recurse
	                updateTimeout();
	            }
	            
	        }, 1000, false); // 1 second, do NOT execute a global digest    
	    }
	    
        //get the user
	    userService.getCurrentUser().then(function (user) {
	        $scope.user = user;
	        if ($scope.user) {
	            $scope.remainingAuthSeconds = $scope.user.remainingAuthSeconds;
	            $scope.canEditProfile = _.indexOf($scope.user.allowedSections, "users") > -1;
	            //set the timer
	            updateTimeout();
	        }
	    });

        
        
    });