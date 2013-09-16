angular.module('umbraco').controller("Umbraco.Editors.UserPickerController", 
	function($rootScope, $scope, $log, userResource){

    userResource.getAll().then(function (userArray) {
        $scope.users = userArray;
    });
    	    
    if ($scope.model.value === null || $scope.model.value === undefined) {
        $scope.model.value = "";
    }
});