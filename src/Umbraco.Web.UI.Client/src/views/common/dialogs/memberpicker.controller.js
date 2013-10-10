//used for the member picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.MemberPickerController",
	function ($scope, eventsService, searchService, $log) {	
	var dialogOptions = $scope.$parent.dialogOptions;
	$scope.dialogTreeEventHandler = $({});
	$scope.results = [];

	$scope.performSearch = function(){
		if($scope.term){
			searchService.searchMembers({term: $scope.term, results: $scope.results});
			$scope.showSearch = true;
		}else{
			$scope.showSearch = false;
		}
		
	};


	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();

		eventsService.publish("Umbraco.Dialogs.MemberPickerController.Select", args).then(function(args){
			if(dialogOptions && dialogOptions.multipicker){
				
				var c = $(args.event.target.parentElement);
				if(!args.node.selected){
					args.node.selected = true;
					c.find("i.umb-tree-icon").hide()
					.after("<i class='icon umb-tree-icon sprTree icon-check blue temporary'></i>");
				}else{
					args.node.selected = false;
					c.find(".temporary").remove();
					c.find("i.umb-tree-icon").show();
				}
				$scope.select(args.node);

			}else{
				$scope.submit(args.node);					
			}
			
		});

	});
});