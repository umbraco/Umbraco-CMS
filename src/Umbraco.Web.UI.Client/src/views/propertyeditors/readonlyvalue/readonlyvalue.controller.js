angular.module('umbraco').controller("Umbraco.Editors.ReadOnlyValueController",
	function($rootScope, $scope, $filter){

	    if ($scope.model.config &&
	        angular.isArray($scope.model.config) &&
	        $scope.model.config.length > 0 &&
	        $scope.model.config[0] &&
	        $scope.model.config.filter)
	    {
	        
	        if ($scope.model.config.format) {
	            $scope.displayvalue = $filter($scope.model.config.filter)($scope.model.value, $scope.model.config.format);
	        }
	        else {
	            $scope.displayvalue = $filter($scope.model.config.filter)($scope.model.value);
	        }
	    }
	    else {
			$scope.displayvalue = $scope.model.value;
		}
});