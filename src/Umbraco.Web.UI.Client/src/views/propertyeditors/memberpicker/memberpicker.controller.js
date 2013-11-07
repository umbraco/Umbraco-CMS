//this controller simply tells the dialogs service to open a memberPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.PropertyEditors.MemberPickerController",
	
	function($scope, dialogService, entityResource, $log, iconHelper){
		$scope.renderModel = [];
		$scope.ids = $scope.model.value.split(',');

		$scope.cfg = {multiPicker: false, entityType: "Member", type: "member", treeAlias: "member", filter: ""};
		if($scope.model.config){
			$scope.cfg = angular.extend($scope.cfg, $scope.model.config);
		}

		entityResource.getByIds($scope.ids, $scope.cfg.entityType).then(function(data){
		    _.each(data, function (item, i) {
				item.icon = iconHelper.convertFromLegacyIcon(item.icon);
				$scope.renderModel.push({name: item.name, id: item.id, icon: item.icon});
			});
		});

		$scope.openMemberPicker =function(){
				var d = dialogService.memberPicker(
							{
								scope: $scope, 
								multiPicker: $scope.cfg.multiPicker,
								filter: $scope.cfg.filter,
								filterCssClass: "not-allowed", 
								callback: populate}
								);
		};


		$scope.remove =function(index){
			$scope.renderModel.splice(index, 1);
			$scope.ids.splice(index, 1);
			$scope.model.value = trim($scope.ids.join(), ",");
		};

		$scope.add =function(item){
			if($scope.ids.indexOf(item.id) < 0){
				item.icon = iconHelper.convertFromLegacyIcon(item.icon);

				$scope.ids.push(item.id);
				$scope.renderModel.push({name: item.name, id: item.id, icon: item.icon});
				$scope.model.value = trim($scope.ids.join(), ",");
			}	
		};

	    $scope.clear = function() {
	        $scope.model.value = "";
	        $scope.renderModel = [];
	        $scope.ids = [];
	    };
	   

	    $scope.sortableOptions = {
	        update: function(e, ui) {
	        	var r = [];
	        	angular.forEach($scope.renderModel, function(value, key){
	        		r.push(value.id);
	        	});

	        	$scope.ids = r;
	        	$scope.model.value = trim($scope.ids.join(), ",");
	        }
	    };


	    $scope.$on("formSubmitting", function (ev, args) {
			$scope.model.value = trim($scope.ids.join(), ",");
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