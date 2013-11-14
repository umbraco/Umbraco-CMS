angular.module("umbraco")
	.controller("Umbraco.Editors.Content.CopyController",
	function ($scope, eventsService, contentResource, navigationService, $log) {	
	var dialogOptions = $scope.$parent.dialogOptions;
	
	$scope.dialogTreeEventHandler = $({});
	var node = dialogOptions.currentNode;

	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();

		eventsService.publish("Umbraco.Editors.Content.CopyController.Select", args);
	    
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

	$scope.copy = function(){
		contentResource.copy({parentId: $scope.target.id, id: node.id, relateToOriginal: $scope.relate})
			.then(function(path){
				$scope.error = false;
				$scope.success = true;

				navigationService.syncTree({ tree: "content", path: path, forceReload: true });

			},function(err){
				$scope.success = false;
				$scope.error = err;
			});
	};
});