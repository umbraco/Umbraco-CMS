angular.module("umbraco.install").controller("Umbraco.InstallerController",
	function ($scope, installerService) {

	    // TODO: Decouple the service from the controller - the controller should be responsible
	    // for the model (state) and the service should be responsible for helping the controller,
	    // the controller should be passing the model into it's methods for manipulation and not hold
	    // state. We should not be assigning properties from a service to a controller's scope.
	    // see: https://github.com/umbraco/Umbraco-CMS/commit/b86ef0d7ac83f699aee35d807f7f7ebb6dd0ed2c#commitcomment-5721204

	    $scope.stepIndex = 0;
	    //comment this out if you just want to see tips
	    installerService.init();

	    //uncomment this to see tips
	    //installerService.switchToFeedback();

	    $scope.installer = installerService.status;

	    $scope.forward = function () {
	        installerService.forward();
	    };

	    $scope.backward = function () {
	        installerService.backward();
	    };

	    $scope.install = function () {
	        installerService.install();
	    };

	    $scope.gotoStep = function (step) {
	        installerService.gotoNamedStep(step);
	    };

	    $scope.restart = function () {
	        installerService.gotoStep(0);
	    };
	});

//this ensure that we start with a clean slate on every install and upgrade
angular.module("umbraco.install").run(function ($templateCache) {
    $templateCache.removeAll();
});