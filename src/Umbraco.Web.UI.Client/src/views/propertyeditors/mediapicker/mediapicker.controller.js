//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.Editors.MediaPickerController", 
	function($rootScope, $scope, dialogService, $log){
    $scope.openMediaPicker =function(value){
            var d = dialogService.mediaPicker({scope: $scope, callback: populate});
    };

    function populate(data){
    	$scope.model.value = data.selection;
    }
});