//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.TreePickerController",
	function ($scope, entityResource, eventsService, $log, searchService, angularHelper, $timeout, localizationService) {

	    var tree = null;
	    var dialogOptions = $scope.dialogOptions;
	    $scope.dialogTreeEventHandler = $({});
	    $scope.section = dialogOptions.section;
	    $scope.treeAlias = dialogOptions.treeAlias;
	    $scope.multiPicker = dialogOptions.multiPicker;
	    $scope.hideHeader = true; 	    	    
        $scope.searchInfo = {
            searchFrom: dialogOptions.startNodeId,
            searchFromName: null,
            showSearch: false,
            results: []
        }

	    //create the custom query string param for this tree
	    $scope.customTreeParams = dialogOptions.startNodeId ? "startNodeId=" + dialogOptions.startNodeId : "";
	    $scope.customTreeParams += dialogOptions.customTreeParams ? "&" + dialogOptions.customTreeParams : "";

	    var entityType = "Document";
	    

	    //min / max values
	    if (dialogOptions.minNumber) {
	        dialogOptions.minNumber = parseInt(dialogOptions.minNumber, 10);
	    }
	    if (dialogOptions.maxNumber) {
	        dialogOptions.maxNumber = parseInt(dialogOptions.maxNumber, 10);
	    }

	    if (dialogOptions.section === "member") {
	        entityType = "Member";            
	    }
	    else if (dialogOptions.section === "media") {	    
	        entityType = "Media";
	    }

	    //Configures filtering
	    if (dialogOptions.filter) {

	        dialogOptions.filterExclude = false;
	        dialogOptions.filterAdvanced = false;

	        if (dialogOptions.filter[0] === "!") {
	            dialogOptions.filterExclude = true;
	            dialogOptions.filter = dialogOptions.filter.substring(1);
	        }

	        //used advanced filtering
	        if (dialogOptions.filter[0] === "{") {
	            dialogOptions.filterAdvanced = true;
	        }
	    } 

	    function nodeSearchHandler(ev, args) {
            if (args.node.metaData.isContainer === true) {
                $scope.searchInfo.showSearch = true;
                $scope.searchInfo.searchFromName = args.node.name;
                $scope.searchInfo.searchFrom = args.node.id;
            }
        }

	    function nodeExpandedHandler(ev, args) {            
	        if (angular.isArray(args.children)) {

	            //now we need to look in the already selected search results and 
	            // toggle the check boxes for those ones that are listed
	            _.each(args.children, function (result) {
	                var exists = _.find($scope.dialogData.selection, function (selectedId) {
	                    return result.id == selectedId;
	                });
	                if (exists) {
	                    result.selected = true;
	                }
	            });

	            //check filter
	            if (dialogOptions.filter) {	             
	                performFiltering(args.children);	             
	            }
	        }
	    }

        //gets the tree object when it loads
	    function treeLoadedHandler(ev, args) {
	        tree = args.tree;
	    }

	    //wires up selection
	    function nodeSelectHandler(ev, args) {
	        args.event.preventDefault();
	        args.event.stopPropagation();

	        eventsService.emit("dialogs.treePickerController.select", args);

	        if (args.node.filtered) {
	            return;
	        }

	        //This is a tree node, so we don't have an entity to pass in, it will need to be looked up
	        //from the server in this method.
	        select(args.node.name, args.node.id);

            //toggle checked state
	        args.node.selected = args.node.selected === true ? false : true;
	    }

	    /** Method used for selecting a node */
	    function select(text, id, entity) {
	        //if we get the root, we just return a constructed entity, no need for server data
	        if (id < 0) {
	            if ($scope.multiPicker) {
	                $scope.select(id);
	            }
	            else {
	                var node = {
	                    alias: null,
	                    icon: "icon-folder",
	                    id: id,
	                    name: text
	                };
	                $scope.submit(node);
	            }
	        }
	        else {
	            
	            if ($scope.multiPicker) {
	                $scope.select(Number(id));
	            }
	            else {
                    
	                $scope.hideSearch();

	                //if an entity has been passed in, use it
	                if (entity) {
	                    $scope.submit(entity);
	                } else {
	                    //otherwise we have to get it from the server
	                    entityResource.getById(id, entityType).then(function (ent) {
	                        $scope.submit(ent);
	                    });
	                }
	            }
	        }
	    }

	    function performFiltering(nodes) {
	        if (dialogOptions.filterAdvanced) {
	            angular.forEach(_.where(nodes, angular.fromJson(dialogOptions.filter)), function (value, key) {
	                value.filtered = true;
	                if (dialogOptions.filterCssClass) {
	                    value.cssClasses.push(dialogOptions.filterCssClass);
	                }
	            });
	        } else {
	            var a = dialogOptions.filter.split(',');
	            angular.forEach(nodes, function (value, key) {

	                var found = a.indexOf(value.metaData.contentType) >= 0;

	                if (!dialogOptions.filterExclude && !found || dialogOptions.filterExclude && found) {
	                    value.filtered = true;

	                    if (dialogOptions.filterCssClass) {
	                        value.cssClasses.push(dialogOptions.filterCssClass);
	                    }
	                }
	            });
	        }
	    }
        
	    $scope.multiSubmit = function (result) {
	        entityResource.getByIds(result, entityType).then(function (ents) {
	            $scope.submit(ents);
	        });
	    };
        
	    /** method to select a search result */
	    $scope.selectResult = function (evt, result) {
	        result.selected = result.selected === true ? false : true;

	        //since result = an entity, we'll pass it in so we don't have to go back to the server
	        select(result.name, result.id, result);
	    };

	    $scope.hideSearch = function () {
            
	        //TODO: Move this to the treeService, we don't need a reference to the 'tree' the way this is working
	        // because if we have a single node, that is all we need since we can traverse to the tree root in the treeService.
            // this logic needs to be centralized so it can be used in other tree + search areas.

	        if (tree) {

	            //we need to ensure that any currently displayed nodes that get selected
	            // from the search get updated to have a check box!
                function checkChildren(children) {
                    _.each(children, function (result) {
                        //check if the id is in the selection, if so ensure it's flagged as selected
                        var exists = _.find($scope.dialogData.selection, function (selectedId) {
                            return result.id == selectedId;
                        });
                        if (exists) {
                            result.selected = true;
                        }
                        else {
                            //check if the node is selected, if its not found then unselect it
                            result.selected = false;
                        }
                        

                        //recurse
                        if (result.children && result.children.length > 0) {
                            checkChildren(result.children);
                        }
                    });
                }
                checkChildren(tree.root.children);
	        }
	        

            $scope.searchInfo.showSearch = false;
            $scope.searchInfo.searchFromName = dialogOptions.startNodeId;
            $scope.searchInfo.searchFrom = null;
            $scope.searchInfo.results = [];
        }

	    $scope.onSearchResults = function(results) {
	        $scope.searchInfo.results = results;

	        _.each($scope.searchInfo.results, function (result) {
	            var exists = _.find($scope.dialogData.selection, function (selectedId) {
	                return result.id == selectedId;
	            });
	            if (exists) {
	                result.selected = true;
	            }
	        });

	        $scope.searchInfo.showSearch = true;
	    };

	    $scope.dialogTreeEventHandler.bind("treeLoaded", treeLoadedHandler);
	    $scope.dialogTreeEventHandler.bind("treeNodeSearch", nodeSearchHandler);
	    $scope.dialogTreeEventHandler.bind("treeNodeExpanded", nodeExpandedHandler);
	    $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);

	    $scope.$on('$destroy', function () {
	        $scope.dialogTreeEventHandler.unbind("treeLoaded", treeLoadedHandler);
	        $scope.dialogTreeEventHandler.unbind("treeNodeSearch", nodeSearchHandler);
	        $scope.dialogTreeEventHandler.unbind("treeNodeExpanded", nodeExpandedHandler);
	        $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
	    });
	});