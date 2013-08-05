//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.ContentPickerController",
	function ($scope) {	
	$scope.dialogTreeEventHandler = $({});

	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();
			
		$(args.event.target.parentElement).find("i.umb-tree-icon").attr("class", "icon umb-tree-icon sprTree icon-check blue");
		$scope.select(args.node);
	});
});