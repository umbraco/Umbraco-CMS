//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.PrevalueEditors.TreePickerController",
	
	function($scope, dialogService, entityResource, $log, iconHelper){
		$scope.renderModel = [];
		$scope.ids = [];


	    var config = {
	        multiPicker: false,
	        entityType: "Document",
	        type: "content",
	        treeAlias: "content"
	    };
		
		if($scope.model.value){
			$scope.ids = $scope.model.value.split(',');
			entityResource.getByIds($scope.ids, config.entityType).then(function (data) {
			    _.each(data, function (item, i) {
					item.icon = iconHelper.convertFromLegacyIcon(item.icon);
					$scope.renderModel.push({name: item.name, id: item.id, icon: item.icon});
				});
			});
		}
		

		$scope.openContentPicker =function() {
		    var d = dialogService.treePicker({
		        section: config.type,
		        treeAlias: config.type,
		        multiPicker: config.multiPicker,
		        callback: populate
		    });
		};

		$scope.remove =function(index){
			$scope.renderModel.splice(index, 1);
			$scope.ids.splice(index, 1);
			$scope.model.value = trim($scope.ids.join(), ",");
		};

		$scope.clear = function() {
		    $scope.model.value = "";
		    $scope.renderModel = [];
		    $scope.ids = [];
		};
		
		$scope.add =function(item){
			if($scope.ids.indexOf(item.id) < 0){
				item.icon = iconHelper.convertFromLegacyIcon(item.icon);

				$scope.ids.push(item.id);
				$scope.renderModel.push({name: item.name, id: item.id, icon: item.icon});
				$scope.model.value = trim($scope.ids.join(), ",");
			}	
		};


	    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
			$scope.model.value = trim($scope.ids.join(), ",");
	    });

	    //when the scope is destroyed we need to unsubscribe
	    $scope.$on('$destroy', function () {
	        unsubscribe();
	    });

		function trim(str, chr) {
			var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^'+chr+'+|'+chr+'+$', 'g');
			return str.replace(rgxtrim, '');
		}

		function populate(data){
			if(angular.isArray(data)){
			    _.each(data, function (item, i) {
					$scope.add(item);
				});
			}else{
				$scope.clear();
				$scope.add(data);
			}
		}
});