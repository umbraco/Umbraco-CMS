angular.module("umbraco.install").controller("Umbraco.Installer.DataBaseController", function($scope, installerService){
	$scope.validateAndForward = function(){
		if(this.myForm.$valid){
			installerService.forward();
		}
	};
});