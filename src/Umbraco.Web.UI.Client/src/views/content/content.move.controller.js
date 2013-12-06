//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Editors.Content.MoveController",
	function ($scope, eventsService, contentResource, navigationService, appState, treeService) {
	var dialogOptions = $scope.$parent.dialogOptions;
	
	$scope.dialogTreeEventHandler = $({});
	var node = dialogOptions.currentNode;

	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();

		eventsService.emit("editors.content.moveController.select", args);
	    
		var c = $(args.event.target.parentElement);

		if ($scope.selectedEl) {
		    $scope.selectedEl.find(".temporary").remove();
		    $scope.selectedEl.find("i.umb-tree-icon").show();
		}

		var temp = "<i class='icon umb-tree-icon sprTree icon-check blue temporary'></i>";
		var icon = c.find("i.umb-tree-icon");
		if (icon.length > 0) {
		    icon.hide().after(temp);
		} else {
		    c.prepend(temp);
		}


		$scope.target = args.node;
		$scope.selectedEl = c;
	});

	$scope.move = function(){
		contentResource.move({parentId: $scope.target.id, id: node.id})
			.then(function(path){
				$scope.error = false;
				$scope.success = true;

			    //first we need to remove the node that launched the dialog
				treeService.removeNode($scope.currentNode);

			    //get the currently edited node (if any)
				var activeNode = appState.getTreeState("selectedNode");

			    //we need to do a double sync here: first sync to the moved content - but don't activate the node,
			    //then sync to the currenlty edited content (note: this might not be the content that was moved!!)
                
				navigationService.syncTree({ tree: "content", path: path, forceReload: true, activate: false }).then(function (args) {
				    if (activeNode) {
				        var activeNodePath = treeService.getPath(activeNode).join();
				        //sync to this node now - depending on what was copied this might already be synced but might not be
				        navigationService.syncTree({ tree: "content", path: activeNodePath, forceReload: false, activate: true });
				    }
				});

			},function(err){
				$scope.success = false;
				$scope.error = err;
			});
	};
});