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
			

			if($scope.multiPicker){
				$scope.select(id);
			}else{
				
				var node = {
					alias: null,
					icon: "icon-folder",
					id: id,
					name: text};

				$scope.submit(node);
			}

		}else{
			$scope.showSearch = false;
			$scope.results = [];
			$scope.term = "";
			$scope.oldTerm = undefined;


			if($scope.multiPicker){
				$scope.select(id);
			}else{
				entityResource.getById(id, $scope.entityType).then(function(ent){
						$scope.submit(ent);
				});
			}
		}
	}

	$scope.multiSubmit = function(result){
		entityResource.getByIds(result, $scope.entityType).then(function(ents){
				$scope.submit(ents);
		});
	};


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

	if(dialogOptions.filter){
		var filterArray = dialogOptions.filter.split(',');

		$scope.dialogTreeEventHandler.bind("treeNodeExpanded", function(ev, args){

			if(angular.isArray(args.children)){
				angular.forEach(args.children, function(value, key){
					if(filterArray.indexOf(value.metaData.contentType) >= 0){
						value.filtered = true;
						value.cssClasses.push("not-allowed");
					}
				});
			}

		});
	}

	$scope.dialogTreeEventHandler.bind("treeNodeSelect", function(ev, args){
		args.event.preventDefault();
		args.event.stopPropagation();

		eventsService.publish("Umbraco.Dialogs.TreePickerController.Select", args).then(function(args){
			
			if(args.node.filtered){
				return;
			}

			select(args.node.name, args.node.id);

			//ui...
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