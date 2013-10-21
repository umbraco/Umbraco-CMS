//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.TreePickerController",
	function ($scope, entityResource, eventsService, $log, searchService) {	
	var dialogOptions = $scope.$parent.dialogOptions;
	$scope.dialogTreeEventHandler = $({});
	$scope.section = dialogOptions.section;
	$scope.treeAlias = dialogOptions.treeAlias;
	$scope.multiPicker = dialogOptions.multiPicker;
	
	//search defaults
	$scope.searcher = searchService.searchContent;
	$scope.entityType ="Document";
	$scope.results = [];

	if(dialogOptions.section === "member"){
		$scope.searcher = searchService.searchMembers;
		$scope.entityType = "Member";
	}else if(dialogOptions.section === "media"){
		$scope.searcher = searchService.searchMedia;
		$scope.entityType = "Media";
	}

	function select(text, id){
		
		//if we get the root, we just return a constructed entity, no need for server data
		if(id < 0){
			var node = {
				alias: null,
				icon: "icon-folder",
				id: id,
				name: text};

			if($scope.multiPicker){
				$scope.select(node);
			}else{
				$scope.submit(node);
			}
		}else{
			entityResource.getById(id, $scope.entityType).then(function(ent){
				if($scope.multiPicker){
					
					$scope.showSearch = false;
					$scope.results = [];
					$scope.term = "";
					$scope.oldTerm = undefined;

					$scope.select(ent);
				}else{
					$scope.submit(ent);
				}
			});
		}
	}

	$scope.selectResult = function(result){
		select(result.title, result.id);
	};

	$scope.performSearch = function(){
		if($scope.term){
			if($scope.oldTerm !== $scope.term){
				$scope.results = [];
				
				$scope.searcher.call(null, {term: $scope.term, results: $scope.results});
				
				$scope.showSearch = true;
				$scope.oldTerm = $scope.term;
			}
		}else{
			$scope.oldTerm = "";
			$scope.showSearch = false;
			$scope.results = [];
		}
	};

	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();

		eventsService.publish("Umbraco.Dialogs.TreePickerController.Select", args).then(function(args){
			
			select(args.node.name, args.node.id);

			if($scope.multiPicker){
				var c = $(args.event.target.parentElement);
				if(!args.node.selected){
					args.node.selected = true;
					var temp = "<i class='icon umb-tree-icon sprTree icon-check blue temporary'></i>";
					var icon = c.find("i.umb-tree-icon");
					if(icon.length > 0){
						icon.hide().after(temp);
					}else{
						c.prepend(temp);
					}
				}else{
					args.node.selected = false;
					c.find(".temporary").remove();
					c.find("i.umb-tree-icon").show();
				}
			}
		});
	});
});