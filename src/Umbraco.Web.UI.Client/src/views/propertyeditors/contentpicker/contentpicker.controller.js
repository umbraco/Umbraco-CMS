//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.PropertyEditors.ContentPickerController",
	
	function($scope, dialogService, entityResource, editorState, $log, iconHelper){
		$scope.renderModel = [];
		$scope.ids = $scope.model.value ? $scope.model.value.split(',') : [];
        
		//configuration
		$scope.cfg = {
			multiPicker: "0", 
			entityType: "Document", 
			filterCssClass: "not-allowed not-published",

			startNode:{
				type: "content",
				id: -1
			}
		};

		if($scope.model.config){
			$scope.cfg = angular.extend($scope.cfg, $scope.model.config);
		}
	    
	    //Umbraco persists boolean for prevalues as "0" or "1" so we need to convert that!
		$scope.cfg.multiPicker = ($scope.cfg.multiPicker === "0" ? false : true);

		if($scope.cfg.startNode.type === "member"){
		    $scope.cfg.entityType = "Member";		    
		}
		else if ($scope.cfg.startNode.type === "media") {
			$scope.cfg.entityType = "Media";
		}

		//if we have a query for the startnode, we will use that. 
		if($scope.cfg.startNode.query){
			var rootId = -1;
			if($scope.cfg.startNode.scope === "current"){
				rootId = editorState.current.id;
			}

			entityResource.getByQuery($scope.cfg.startNode.query, rootId, "Document").then(function(ent){
				$scope.cfg.startNodeId = ent.id;	
			});	
		}else{
			$scope.cfg.startNodeId = $scope.cfg.startNode.id;
		}

		$scope.cfg.callback = populate;
		$scope.cfg.treeAlias = $scope.cfg.startNode.type;
		$scope.cfg.section = $scope.cfg.startNode.type;		
		
		//load current data
		entityResource.getByIds($scope.ids, $scope.cfg.entityType).then(function(data){
		    _.each(data, function (item, i) {
				item.icon = iconHelper.convertFromLegacyIcon(item.icon);
				$scope.renderModel.push({name: item.name, id: item.id, icon: item.icon});
			});
		});


		//dialog
		$scope.openContentPicker =function(){
			var d = dialogService.treePicker($scope.cfg);
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