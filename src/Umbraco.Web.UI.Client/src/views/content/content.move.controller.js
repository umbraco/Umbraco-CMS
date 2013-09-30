//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Editors.Content.MoveController",
	function ($scope, eventsService, $log) {	
	var dialogOptions = $scope.$parent.dialogOptions;
	$scope.dialogTreeEventHandler = $({});
	$scope.section = dialogOptions.section;


	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();

		eventsService.publish("Umbraco.Content.MoveController.Select", args).then(function(args){
			
		});

	});
});