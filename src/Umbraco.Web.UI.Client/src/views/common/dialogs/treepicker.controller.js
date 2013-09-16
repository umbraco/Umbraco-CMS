//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.TreePickerController",
	function ($scope, eventsService, $log) {	
	var dialogOptions = $scope.$parent.dialogOptions;
	$scope.dialogTreeEventHandler = $({});
	$scope.section = dialogOptions.section | "content";

	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();

		eventsService.publish("Umbraco.Dialogs.TreePickerController.Select", args).then(function(args){
			if(dialogOptions && dialogOptions.multipicker){
				$(args.event.target.parentElement)
					.find("i.umb-tree-icon")
					.attr("class", "icon umb-tree-icon sprTree icon-check blue");
				
				$scope.select(args.node);
			}else{
				$scope.submit(args.node);					
			}
			
		});

	});
});