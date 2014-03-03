angular.module("umbraco.install").controller("Umbraco.Install.UserController", function($scope, installerService){

	$scope.validateAndInstall = function(){
			installerService.install();
	};

	$scope.validateAndForward = function(){
			if(this.myForm.$valid){
				installerService.forward();
			}
	};
	
});