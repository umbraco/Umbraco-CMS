//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Editors.Content.MoveController",
	function ($scope, eventsService, contentResource, $log) {	
	var dialogOptions = $scope.$parent.dialogOptions;
	
	$scope.dialogTreeEventHandler = $({});
	var node = dialogOptions.currentNode;

	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();

		eventsService.publish("Umbraco.Content.MoveController.Select", args).then(function(args){
				
				contentResource.move({parentId: args.node.id, id:node.id})
				.then(function(){
					alert("moved");
				},function(err){
					alert(err);
				});
		});

	});
});