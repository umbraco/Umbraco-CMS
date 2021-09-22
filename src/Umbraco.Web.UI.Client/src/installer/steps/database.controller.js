angular.module("umbraco.install").controller("Umbraco.Installer.DataBaseController", function($scope, $http, installerService){
	
	$scope.checking = false;
	$scope.invalidDbDns = false;
			
	$scope.dbs = [
		{ name: 'Microsoft SQL Server Compact (SQL CE)', id: 0 },
		{ name: 'Microsoft SQL Server', id: 1 },
		{ name: 'Microsoft SQL Azure', id: 3 },
		{ name: 'Custom connection string', id: -1 }
	];

    if (Utilities.isUndefined(installerService.status.current.model.dbType) || installerService.status.current.model.dbType === null) {
		installerService.status.current.model.dbType = 0;
	}
	
    $scope.validateAndForward = function() {
        if (!$scope.checking && this.myForm.$valid)
        {
		 	$scope.checking = true;
			$scope.invalidDbDns = false;
			
			var model = installerService.status.current.model;

			$http.post(
				Umbraco.Sys.ServerVariables.installApiBaseUrl + "PostValidateDatabaseConnection",
				model).then(function(response) {
					
					if (response.data === true) {
						installerService.forward();	
					}
					else {
						$scope.invalidDbDns = true;
					}

					$scope.checking = false;
			}, function(){
				$scope.invalidDbDns = true;
				$scope.checking = false;
			});
		}
	};
});
