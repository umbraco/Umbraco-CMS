angular.module("umbraco.install").controller("Umbraco.InstallerController",
	function($scope, installerService){

	$scope.stepIndex = 0;
	installerService.init();
	$scope.installer = installerService.status;

	$scope.forward = function(){
		installerService.forward();
	};

	$scope.backward = function(){
		installerService.backward();
	};

	$scope.install = function(){
		installerService.install();
	};

	$scope.gotoStep = function(step){
		installerService.gotoNamedStep(step);
	};

	$scope.restart = function () {	   
	    installerService.gotoStep(0);
	};
});
