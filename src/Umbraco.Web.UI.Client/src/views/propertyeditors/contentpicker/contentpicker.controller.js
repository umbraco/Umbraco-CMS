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

		$scope.remove =function(index){
			$scope.renderModel.splice(index, 1);
			$scope.ids.splice(index, 1);
			$scope.model.value = $scope.ids.join();
		};

		$scope.add =function(item){

			if($scope.ids.indexOf(item.id) < 0){
				$scope.renderModel.push({name: item.name, id: item.id, icon: item.icon})
				$scope.ids.push(item.id);	

				$scope.model.value = $scope.ids.join();	
			}	
		};


		function populate(data){
			$(data.selection).each(function(i, item){
				$scope.add(item);
			});
		}
});