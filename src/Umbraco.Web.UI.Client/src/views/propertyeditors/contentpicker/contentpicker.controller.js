//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.PropertyEditors.ContentPickerController",

	function($scope, dialogService, entityResource, editorState, $log, iconHelper, $routeParams){
	    $scope.renderModel = [];
	    $scope.ids = $scope.model.value ? $scope.model.value.split(',') : [];
        
	    //configuration
	    $scope.cfg = {
	        multiPicker: "0",
	        entityType: "Document",
			filterCssClass: "not-allowed not-published",

	        startNode: {
				query: "",
	            type: "content",
	            id: -1
	        }
	    };

	    if ($scope.model.config) {
	        $scope.cfg = angular.extend($scope.cfg, $scope.model.config);
	    }

	    //Umbraco persists boolean for prevalues as "0" or "1" so we need to convert that!
	    $scope.cfg.multiPicker = ($scope.cfg.multiPicker === "0" ? false : true);

	    if ($scope.cfg.startNode.type === "member") {
	        $scope.cfg.entityType = "Member";
	    }
	    else if ($scope.cfg.startNode.type === "media") {
	        $scope.cfg.entityType = "Media";
	    }

		//if we have a query for the startnode, we will use that. 
		if($scope.cfg.startNode.query){
			var rootId = $routeParams.id; 
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
	    entityResource.getByIds($scope.ids, $scope.cfg.entityType).then(function (data) {
	        _.each(data, function (item, i) {
	            item.icon = iconHelper.convertFromLegacyIcon(item.icon);
	            $scope.renderModel.push({ name: item.name, id: item.id, icon: item.icon });
	        });
	    });


	    //dialog
	    $scope.openContentPicker = function () {
	        var d = dialogService.treePicker($scope.cfg);
	    };


	    $scope.remove = function (index) {
	        $scope.renderModel.splice(index, 1);
	    };


	    $scope.add = function (item) {
	        if ($scope.ids.indexOf(item.id) < 0) {
	            item.icon = iconHelper.convertFromLegacyIcon(item.icon);
	            $scope.renderModel.push({ name: item.name, id: item.id, icon: item.icon });
	        }
	    };

	    $scope.clear = function () {
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

            //Validate!
	        if ($scope.model.config.minNumber && parseInt($scope.model.config.minNumber) > $scope.renderModel.length) {
	            $scope.contentPickerForm.minCount.$setValidity("minCount", false);
	        }
	        else {
	            $scope.contentPickerForm.minCount.$setValidity("minCount", true);
	        }

	        if ($scope.model.config.maxNumber && parseInt($scope.model.config.maxNumber) < $scope.renderModel.length) {
	            $scope.contentPickerForm.maxCount.$setValidity("maxCount", false);
	        }
	        else {
	            $scope.contentPickerForm.maxCount.$setValidity("maxCount", true);
	        }
	    });

	    $scope.$on("formSubmitting", function (ev, args) {
	        $scope.model.value = trim($scope.ids.join(), ",");
	    });

	    function trim(str, chr) {
	        var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
	        return str.replace(rgxtrim, '');
	    }

	    function populate(data) {
	        if (angular.isArray(data)) {
	            _.each(data, function (item, i) {
	                $scope.add(item);
	            });
	        } else {
	            $scope.clear();
	            $scope.add(data);
	        }
	    }
	});