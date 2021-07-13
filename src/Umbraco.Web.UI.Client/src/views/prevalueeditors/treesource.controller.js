//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.PrevalueEditors.TreeSourceController",
	
	function($scope, $timeout, entityResource, iconHelper, editorService, eventsService){

	    if (!$scope.model) {
	        $scope.model = {};
	    }
	    if (!$scope.model.value) {
	        $scope.model.value = {
	            type: "content"
	        };
        }

        // setup the default config
        var config = {
            idType: "udi",
            showXpath: true,
            allowSelectNode: true
        };

        // map the user config
        Utilities.extend(config, $scope.model.config);

        // map back to the model
        $scope.model.config = config;

        if ($scope.model.value.id && $scope.model.value.type !== "member"){
            entityResource.getById($scope.model.value.id, entityType()).then(function(item){
                populate(item);
            });
        }
        else {
            $timeout(function () {
                treeSourceChanged();
            }, 100);
        }

        function entityType() {
			var ent = "Document";
            if($scope.model.value.type === "media"){
                ent = "Media";
            }
            else if ($scope.model.value.type === "member") {
                ent = "Member";
            }
            return ent;
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
			$scope.model.value.id = null;
            $scope.node = null;
            $scope.model.value.query = null;

		    treeSourceChanged();
		};

        function treeSourceChanged() {
            eventsService.emit("treeSourceChanged", { value: $scope.model.value.type });
        }

		//we always need to ensure we dont submit anything broken
	    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
	    	if ($scope.model.value.type === "member"){
	    		$scope.model.value.id = null;
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
            $scope.node.path = "";
            $scope.model.value.id = $scope.model.config.idType === "udi" ? item.udi : item.id;
            entityResource.getUrl(item.id, entityType()).then(function (data) {
                $scope.node.path = data;
            });
		}
});
