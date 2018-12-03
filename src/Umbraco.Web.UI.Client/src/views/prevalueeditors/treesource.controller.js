//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.PrevalueEditors.TreeSourceController",
	
	function($scope, entityResource, iconHelper, editorService){

	    if (!$scope.model) {
	        $scope.model = {};
	    }
	    if (!$scope.model.value) {
	        $scope.model.value = {
	            type: "content"
	        };
        }
        if (!$scope.model.config) {
            $scope.model.config = {
                idType: "int"
            };
        }

		if($scope.model.value.id && $scope.model.value.type !== "member"){
			var ent = "Document";
			if($scope.model.value.type === "media"){
				ent = "Media";
			}
			
			entityResource.getById($scope.model.value.id, ent).then(function(item){
				item.icon = iconHelper.convertFromLegacyIcon(item.icon);
				$scope.node = item;
			});
		}


		$scope.openContentPicker =function(){
			var treePicker = {
                idType: $scope.model.config.idType,
				section: $scope.model.value.type,
				treeAlias: $scope.model.value.type,
				multiPicker: false,
				submit: function(model) {
					var item = model.selection[0];
					populate(item);
					editorService.close();
				},
				close: function() {
					editorService.close();
				}
			};
			editorService.treePicker(treePicker);
		};

		$scope.clear = function() {
			$scope.model.value.id = undefined;
			$scope.node = undefined;
			$scope.model.value.query = undefined;
		};
		

		//we always need to ensure we dont submit anything broken
	    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
	    	if($scope.model.value.type === "member"){
	    		$scope.model.value.id = -1;
	    		$scope.model.value.query = "";
	    	}
	    });

	    //when the scope is destroyed we need to unsubscribe
	    $scope.$on('$destroy', function () {
	        unsubscribe();
	    });

		function populate(item){
				$scope.clear();
				item.icon = iconHelper.convertFromLegacyIcon(item.icon);
				$scope.node = item;
                $scope.model.value.id = $scope.model.config.idType === "udi" ? item.udi : item.id;
		}
});