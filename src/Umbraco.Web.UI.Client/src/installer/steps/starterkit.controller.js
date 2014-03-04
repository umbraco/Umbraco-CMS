angular.module("umbraco.install").controller("Umbraco.Installer.PackagesController", function($scope, installerService){
		
	installerService.getPackages().then(function(response){
		$scope.packages = response.data;
	});

	$scope.setPackageAndContinue = function(package){
		
		installerService.install();
	};
});