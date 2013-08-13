//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.ContentPickerController",
	function ($scope, eventsService) {	
	$scope.dialogTreeEventHandler = $({});

	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();
		
		eventsService.publish("Umbraco.Dialogs.ContentPickerController.Select", args, "hello").then(function(args){
			$(args.event.target.parentElement).find("i.umb-tree-icon").attr("class", "icon umb-tree-icon sprTree icon-check blue");
			$scope.select(args.node);
		});		
	});
});