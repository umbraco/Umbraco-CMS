function ContentEditDialogController($scope) {
    //setup scope vars
    $scope.model = {};
    $scope.model.defaultButton = null;
    $scope.model.subButtons = [];
    
    var dialogOptions = $scope.$parent.dialogOptions;
    if(dialogOptions.entity){
    	$scope.model.entity = dialogOptions.entity;
    	$scope.loaded = true;	
    }
}

angular.module("umbraco")
	.controller("Umbraco.Dialogs.Content.EditController", ContentEditDialogController);