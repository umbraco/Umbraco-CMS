//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.LinkPickerController",
	function ($scope, eventsService, dialogService, entityResource, contentResource, mediaHelper, $log) {
	var dialogOptions = $scope.$parent.dialogOptions;
	
	$scope.dialogTreeEventHandler = $({});
	$scope.target = {};

	if(dialogOptions.currentTarget){
		$scope.target = dialogOptions.currentTarget;

		//if we a node ID, we fetch the current node to build the form data
		if($scope.target.id){

			if(!$scope.target.path) {
			    entityResource.getPath($scope.target.id, "Document").then(function (path) {
			        $scope.target.path = path;
			        //now sync the tree to this path
			        $scope.dialogTreeEventHandler.syncTree({ path: $scope.target.path, tree: "content" });
			    });
			}

			contentResource.getNiceUrl($scope.target.id).then(function(url){
				$scope.target.url = url;
			});
		}
	}

	$scope.switchToMediaPicker = function(){
		dialogService.mediaPicker({callback: function(media){
					$scope.target.id = undefined;
					$scope.target.name = media.name;
					$scope.target.url = mediaHelper.getMediaPropertyValue({mediaModel: media});
				}});
	};


	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();

		eventsService.emit("dialogs.linkPicker.select", args);
	    
		var c = $(args.event.target.parentElement);

	    //renewing
		if (args.node !== $scope.target) {
		    if ($scope.selectedEl) {
		        $scope.selectedEl.find(".temporary").remove();
		        $scope.selectedEl.find("i.umb-tree-icon").show();
		    }

		    $scope.selectedEl = c;
		    $scope.target.id = args.node.id;
		    $scope.target.name = args.node.name;

		    $scope.selectedEl.find("i.umb-tree-icon")
             .hide()
             .after("<i class='icon umb-tree-icon sprTree icon-check blue temporary'></i>");

		    if (args.node.id < 0) {
		        $scope.target.url = "/";
		    } else {
		        contentResource.getNiceUrl(args.node.id).then(function (url) {
		            $scope.target.url = url;
		        });
		    }
		} else {
		    $scope.target = undefined;
		    //resetting
		    if ($scope.selectedEl) {
		        $scope.selectedEl.find(".temporary").remove();
		        $scope.selectedEl.find("i.umb-tree-icon").show();
		    }
		}

	});
});