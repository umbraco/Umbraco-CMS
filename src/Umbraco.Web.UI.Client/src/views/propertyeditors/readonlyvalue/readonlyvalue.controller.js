angular.module('umbraco').controller("Umbraco.Editors.ReadOnlyValueController",
	function($rootScope, $scope, $filter){
	    if ($scope.config && $scope.config.filter && $scope.config.format) {
	        $scope.displayvalue = $filter($scope.config.filter)($scope.value, $scope.config.filter);
		}else{
			$scope.displayvalue = $scope.value;
		}
});