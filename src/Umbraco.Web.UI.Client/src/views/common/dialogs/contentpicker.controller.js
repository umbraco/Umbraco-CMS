//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.ContentPickerController",
	function ($scope, eventsService, $log) {	
	var dialogOptions = $scope.$parent.dialogOptions;
	$scope.dialogTreeEventHandler = $({});

	$log.log($scope);

	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();
		

		eventsService.publish("Umbraco.Dialogs.ContentPickerController.Select", args, "hello").then(function(args){
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