//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.ContentPickerController",
	function ($scope, eventsService, entityResource, searchService, $log) {	
	var dialogOptions = $scope.$parent.dialogOptions;
	$scope.dialogTreeEventHandler = $({});
	$scope.results = [];

	$scope.selectResult = function(result){
		entityResource.getById(result.id, "Document").then(function(ent){
			if(dialogOptions && dialogOptions.multiPicker){
				
				$scope.showSearch = false;
				$scope.results = [];
				$scope.term = "";
				$scope.oldTerm = undefined;

				$scope.select(ent);
			}else{
				$scope.submit(ent);
			}
		});
	};

	$scope.performSearch = function(){
		if($scope.term){
			if($scope.oldTerm !== $scope.term){
				$scope.results = [];
			    searchService.searchContent({ term: $scope.term }).then(function(data) {
			        $scope.results = data;
			    });
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

		eventsService.emit("dialogs.contentPicker.select", args);
	    
		if (dialogOptions && dialogOptions.multiPicker) {

		    var c = $(args.event.target.parentElement);
		    if (!args.node.selected) {
		        args.node.selected = true;

		        var temp = "<i class='icon umb-tree-icon sprTree icon-check blue temporary'></i>";
		        var icon = c.find("i.umb-tree-icon");
		        if (icon.length > 0) {
		            icon.hide().after(temp);
		        } else {
		            c.prepend(temp);
		        }

		    } else {
		        args.node.selected = false;
		        c.find(".temporary").remove();
		        c.find("i.umb-tree-icon").show();
		    }

		    $scope.select(args.node);

		} else {
		    $scope.submit(args.node);
		}

	});
});