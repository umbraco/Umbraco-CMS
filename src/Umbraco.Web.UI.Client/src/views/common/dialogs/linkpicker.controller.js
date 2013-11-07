//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.LinkPickerController",
	function ($scope, eventsService, entityResource, contentResource, $log) {
	var dialogOptions = $scope.$parent.dialogOptions;
	
	$scope.dialogTreeEventHandler = $({});
	$scope.target = {};

	if(dialogOptions.currentTarget){
		$scope.target = dialogOptions.currentTarget;

		//if we a node ID, we fetch the current node to build the form data
		if($scope.target.id){

			if(!$scope.target.path) {
			    entityResource.getPath($scope.target.id, "Document").then(function (path) {
			        $scope.target.path = path;
			    });
			}

			contentResource.getNiceUrl($scope.target.id).then(function(url){
				$scope.target.url = angular.fromJson(url);
			});
		}
	}


	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();

		eventsService.publish("Umbraco.Dialogs.LinkPickerController.Select", args).then(function(args){
				var c = $(args.event.target.parentElement);

				//renewing
				if(args.node !== $scope.target){
					if($scope.selectedEl){
						$scope.selectedEl.find(".temporary").remove();
						$scope.selectedEl.find("i.umb-tree-icon").show();
					}

					$scope.selectedEl = c;
					$scope.target = args.node;
					$scope.target.name = args.node.name;

					$scope.selectedEl.find("i.umb-tree-icon")
					 .hide()
					 .after("<i class='icon umb-tree-icon sprTree icon-check blue temporary'></i>");
					
					if(args.node.id < 0){
						$scope.target.url = "/";
					}else{
						contentResource.getNiceUrl(args.node.id).then(function(url){
							$scope.target.url = angular.fromJson(url);
						});
					}
				}else{
					$scope.target = undefined;
					//resetting
					if($scope.selectedEl){
						$scope.selectedEl.find(".temporary").remove();
						$scope.selectedEl.find("i.umb-tree-icon").show();
					}
				}
		});

	});
});