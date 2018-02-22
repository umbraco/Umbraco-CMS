angular.module("umbraco.install").controller("Umbraco.Install.UserController", function($scope, installerService) {
    
    $scope.passwordPattern = /.*/;
    $scope.installer.current.model.subscribeToNewsLetter = true;
    
    if ($scope.installer.current.model.minNonAlphaNumericLength > 0) {
        var exp = "";
        for (var i = 0; i < $scope.installer.current.model.minNonAlphaNumericLength; i++) {
            exp += ".*[\\W].*";
        }
        //replace duplicates
        exp = exp.replace(".*.*", ".*");            
        $scope.passwordPattern = new RegExp(exp);
    }

	$scope.validateAndInstall = function(){
			installerService.install();
	};

	$scope.validateAndForward = function(){
			if(this.myForm.$valid){
				installerService.forward();
			}
    };
    $scope.togglePassword = function () {
        var elem = $("form[name='myForm'] input[name='installer.current.model.password']");
        elem.attr("type", (elem.attr("type") === "text" ? "password" : "text"));
    }
	
});
