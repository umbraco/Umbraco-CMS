//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.LinkPickerController",
	function ($scope, eventsService, $log) {	
	var dialogOptions = $scope.$parent.dialogOptions;
	$scope.dialogTreeEventHandler = $({});

	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();

		eventsService.publish("Umbraco.Dialogs.LinkPickerController.Select", args).then(function(args){
				var c = $(args.event.target.parentElement);
				if($scope.selectedEl){
					$scope.selectedEl.find(".temporary").remove();
					$scope.selectedEl.find("i.umb-tree-icon").show();
				}

				c.find("i.umb-tree-icon").hide()
				.after("<i class='icon umb-tree-icon sprTree icon-check blue temporary'></i>");
				
				$scope.target = args.node;
				$scope.selectedEl = c;
		});

	});
});