//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.PropertyEditors.ContentPickerController",
	
	function($scope, dialogService, entityResource, $log, iconHelper){
		$scope.ids = $scope.model.value.split(',');

		$scope.renderModel = [];
		$scope.cfg = {multipicker: false, type: "content", filter: ""};

		if($scope.model.config){
			$scope.cfg = $scope.model.config;
		}

		entityResource.getByIds($scope.ids, "Document").then(function(data){
			$(data).each(function(i, item){
				item.icon = iconHelper.convertFromLegacyIcon(item.icon);
				$scope.renderModel.push({name: item.name, id: item.id, icon: item.icon});
			});
		});

		$scope.openContentPicker =function(){
			var d = dialogService.treePicker({
								section: $scope.cfg.type,
								treeAlias: $scope.cfg.type,
								scope: $scope, 
								multiPicker: $scope.cfg.multiPicker,
								filter: $scope.cfg.filter, 
								callback: populate});
		};

		$scope.remove =function(index){
			$scope.renderModel.splice(index, 1);
			$scope.ids.splice(index, 1);
			$scope.model.value = trim($scope.ids.join(), ",");
		};

		$scope.add =function(item){
			if($scope.ids.indexOf(item.id) < 0){
				item.icon = iconHelper.convertFromLegacyIcon(item.icon);
				$scope.renderModel.push({name: item.name, id: item.id, icon: item.icon});
				$scope.ids.push(item.id);
				$scope.model.value = trim($scope.ids.join(), ",");
			}	
		};

	    $scope.clear = function() {
	        $scope.ids = [];
	        $scope.model.value = "";
	        $scope.renderModel = [];
	    };

		function trim(str, chr) {
			var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^'+chr+'+|'+chr+'+$', 'g');
			return str.replace(rgxtrim, '');
		}

		function populate(data){
			if(data.selection && angular.isArray(data.selection)){
				$(data.selection).each(function(i, item){
					$scope.add(item);
				});
			}else{
				$scope.clear();
				$scope.add(data);
			}
		}
});