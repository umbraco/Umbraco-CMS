//this controller simply tells the dialogs service to open a memberPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.PropertyEditors.MemberGroupPickerController",
	
	function($scope, dialogService){
		$scope.renderModel = [];
		$scope.ids = [];

		
	    
	    if ($scope.model.value) {
	        $scope.ids = $scope.model.value.split(',');

	        $($scope.ids).each(function (i, item) {
	           
	            $scope.renderModel.push({ name: item, id: item, icon: 'icon-users' });
	        });
	    }
	    
	    $scope.cfg = {multiPicker: true, entityType: "MemberGroup", type: "membergroup", treeAlias: "memberGroup", filter: ""};
		if($scope.model.config){
			$scope.cfg = angular.extend($scope.cfg, $scope.model.config);
		}

		

		$scope.openMemberGroupPicker =function(){
				var d = dialogService.memberGroupPicker(
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
			if($scope.ids.indexOf(item) < 0){
				//item.icon = iconHelper.convertFromLegacyIcon(item.icon);

				$scope.ids.push(item);
				$scope.renderModel.push({ name: item, id: item, icon: 'icon-users' });
				$scope.model.value = trim($scope.ids.join(), ",");
			}	
		};

	    $scope.clear = function() {
	        $scope.model.value = "";
	        $scope.renderModel = [];
	        $scope.ids = [];
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