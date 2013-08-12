angular.module('umbraco').controller("Umbraco.Editors.ReadOnlyValueController",
	function($rootScope, $scope, $filter){
	    if ($scope.model.config && $scope.model.config.filter && $scope.model.config.format) {
	        $scope.displayvalue = $filter($scope.model.config.filter)($scope.model.value, $scope.model.config.filter);
		}else{
			$scope.displayvalue = $scope.model.value;
		}
});