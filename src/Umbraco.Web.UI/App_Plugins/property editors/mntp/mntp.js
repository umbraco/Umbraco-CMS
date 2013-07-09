//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("uComponents.Editors.MNTPController",
	function($scope, dialogService){
		
    $scope.openContentPicker =function(value){
            var d = dialogService.contentPicker({scope: $scope, callback: populate});
            var s = d;
    };

    function populate(data){
        $scope.model.value = data.selection;
    }
});