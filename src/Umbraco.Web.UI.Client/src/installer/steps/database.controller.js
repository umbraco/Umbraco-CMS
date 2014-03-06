angular.module("umbraco.install").controller("Umbraco.Installer.DataBaseController", function($scope, $http, installerService){
	
	$scope.checking = false;
    $scope.validateAndForward = function(){
		if(!$scope.checking && this.myForm.$valid){
		 	$scope.checking = true;
			var model = installerService.status.current.model;

			$http.post(Umbraco.Sys.ServerVariables.installApiBaseUrl + "PostValidateDatabaseConnection",
				model).then(function(response){
					
					if(response.data === "true"){
						installerService.forward();	
					}else{
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