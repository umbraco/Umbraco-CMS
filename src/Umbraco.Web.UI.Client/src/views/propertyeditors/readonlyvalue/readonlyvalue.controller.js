angular.module('umbraco').controller("Umbraco.Editors.ReadOnlyValueController",
	function($rootScope, $scope, $filter){
		if($scope.config){
			var config = angular.fromJson($scope.config);
			if(config && config.filter){
				$scope.displayvalue = $filter(config.filter)($scope.value, config.format);
			}
		}else{
			$scope.displayvalue = $scope.value;
		}
});