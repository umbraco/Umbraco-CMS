//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.Editors.ContentPickerController",
	
	function($scope, dialogService, entityResource){
		$scope.ids = $scope.model.value.split(',');
		$scope.renderModel = [];

		entityResource.getByIds($scope.ids).then(function(data){
			$(data).each(function(i, item){
				$scope.renderModel.push({name: item.name, id: item.id, icon: item.icon});
			});
		});

		$scope.openContentPicker =function(){
			var d = dialogService.contentPicker({scope: $scope, callback: populate});
		};

		function populate(data){
			$(data.selection).each(function(i, item){
				$scope.renderModel.push({name: item.name, id: item.id, icon: item.icon})
				$scope.ids.push(item.id);	
			});

			//set the model value to a comma-sep string 
			//TOOD: consider if we should save more managed model?
			$scope.model.value = $scope.ids.join();
		}
});