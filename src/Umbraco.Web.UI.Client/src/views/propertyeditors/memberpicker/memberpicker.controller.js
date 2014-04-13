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
		};

		$scope.add =function(item){
			if($scope.ids.indexOf(item.id) < 0){
				item.icon = iconHelper.convertFromLegacyIcon(item.icon);
				$scope.renderModel.push({name: item.name, id: item.id, icon: item.icon});				
			}	
		};

	    $scope.clear = function() {
	        $scope.renderModel = [];
	    };
	   
	    //We need to watch our renderModel so that we can update the underlying $scope.model.value properly, this is required
	    // because the ui-sortable doesn't dispatch an event after the digest of the sort operation. Any of the events for UI sortable
	    // occur after the DOM has updated but BEFORE the digest has occured so the model has NOT changed yet - it even states so in the docs.
	    // In their source code there is no event so we need to just subscribe to our model changes here.
	    //This also makes it easier to manage models, we update one and the rest will just work.
	    $scope.$watch(function () {
	        //return the joined Ids as a string to watch
	        return _.map($scope.renderModel, function (i) {
	            return i.id;
	        }).join();
	    }, function (newVal) {
	        $scope.ids = _.map($scope.renderModel, function (i) {
	            return i.id;
	        });
	        $scope.model.value = trim($scope.ids.join(), ",");
	    });

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