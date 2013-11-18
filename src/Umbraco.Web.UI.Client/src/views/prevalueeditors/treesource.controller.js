//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.PrevalueEditors.TreeSourceController",
	
	function($scope, dialogService, entityResource, $log, iconHelper){

		if(angular.isObject($scope.model.value)){
			$scope.model.type = $scope.model.value.type;
			$scope.model.id = $scope.model.value.id;
		}else{
			$scope.model.type = "content";
		}		

		if($scope.model.id && $scope.model.type !== "member"){
			var ent = "Document";
			if($scope.model.type === "media"){
				ent = "Media";
			}
			
			entityResource.getById($scope.model.id, ent).then(function(item){
				item.icon = iconHelper.convertFromLegacyIcon(item.icon);
				$scope.node = item;
			});
		}


		$scope.openContentPicker =function(){
			var d = dialogService.treePicker({
								section: $scope.model.type,
								treeAlias: $scope.model.type,
								scope: $scope, 
								multiPicker: false,
								callback: populate});
		};

		$scope.clear = function() {
		    $scope.model.id = undefined;
		    $scope.node = undefined;
		};
		
	    $scope.$on("formSubmitting", function (ev, args) {
	    	if($scope.model.type === "member"){
	    		$scope.model.id = -1;
	    	}

			$scope.model.value = {type: $scope.model.type, id: $scope.model.id};
	    });

		function populate(item){
				$scope.clear();
				item.icon = iconHelper.convertFromLegacyIcon(item.icon);
				$scope.node = item;
				$scope.model.id = item.id;
		}
});