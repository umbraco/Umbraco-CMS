angular.module("umbraco.install").controller("Umbraco.Install.UserController", function($scope, installerService) {
    
    $scope.passwordPattern = /.*/;
    $scope.installer.current.model.subscribeToNewsLetter = false;
    
    if ($scope.installer.current.model.minNonAlphaNumericLength > 0) {
        var exp = "";
        for (var i = 0; i < $scope.installer.current.model.minNonAlphaNumericLength; i++) {
            exp += ".*[\\W].*";
        }
        //replace duplicates
        exp = exp.replace(".*.*", ".*");            
        $scope.passwordPattern = new RegExp(exp);
    }

	$scope.validateAndInstall = function() {
	    installerService.install();
	};

	$scope.validateAndForward = function(){
		if (this.myForm.$valid) {
			installerService.forward();
		}
	};
	
});
