//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.TreePickerController",
	function ($scope, entityResource, eventsService, $log, searchService, angularHelper, $timeout, localizationService) {

	    var dialogOptions = $scope.dialogOptions;
	    $scope.dialogTreeEventHandler = $({});
	    $scope.section = dialogOptions.section;
	    $scope.treeAlias = dialogOptions.treeAlias;
	    $scope.multiPicker = dialogOptions.multiPicker;
	    $scope.hideHeader = true; 	    
	    localizationService.localize("general_typeToSearch").then(function (value) {
	        $scope.searchPlaceholderText = value;
	    });
        $scope.searchInfo = {
            selectedSearchResults: [],
            searchFrom: null,
            searchFromName: null,
            showSearch: false,
            term: null,
            oldTerm: null,
            results: []
        }

	    //create the custom query string param for this tree
	    $scope.customTreeParams = dialogOptions.startNodeId ? "startNodeId=" + dialogOptions.startNodeId : "";
	    $scope.customTreeParams += dialogOptions.customTreeParams ? "&" + dialogOptions.customTreeParams : "";

	    //search defaults
	    var searcher = searchService.searchContent;
	    var entityType = "Document";
	    

	    //min / max values
	    if (dialogOptions.minNumber) {
	        dialogOptions.minNumber = parseInt(dialogOptions.minNumber, 10);
	    }
	    if (dialogOptions.maxNumber) {
	        dialogOptions.maxNumber = parseInt(dialogOptions.maxNumber, 10);
	    }

	    //search
	    if (dialogOptions.section === "member") {
	        searcher = searchService.searchMembers;
	        entityType = "Member";            
	    }
	    else if (dialogOptions.section === "media") {
	        searcher = searchService.searchMedia;
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
	            //check filter
	            if (dialogOptions.filter) {	             
	                performFiltering(args.children);	             
	            }
	        }
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

	        toggleCheck(args.event, args.node);
	    }

        function toggleCheck(evt, node) {            
            if ($scope.multiPicker) {
                var c = $(evt.target.parentElement);
                if (!node.selected) {
                    node.selected = true;
                    var temp = "<i class='icon umb-tree-icon sprTree icon-check blue temporary'></i>";
                    var icon = c.find("i.umb-tree-icon");
                    if (icon.length > 0) {
                        icon.hide().after(temp);
                    }
                    else {
                        c.prepend(temp);
                    }
                }
                else {
                    node.selected = false;
                    c.find(".temporary").remove();
                    c.find("i.umb-tree-icon").show();
                }
            }
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
	        } else {
	            
	            if ($scope.multiPicker) {
	                $scope.select(id);
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

            if (result.selected) {
                //add this to the list of selected search results so that when it's re-searched we 
                // can show the checked boxes
                $scope.searchInfo.selectedSearchResults.push(result.id);
            }
            else {
                $scope.searchInfo.selectedSearchResults = _.reject($scope.searchInfo.selectedSearchResults, function(itemId) {
                    return itemId == result.id;
                });
            }
	        
	    };

        $scope.hideSearch = function() {
            $scope.searchInfo.showSearch = false;
            $scope.searchInfo.searchFromName = null;
            $scope.searchInfo.searchFrom = null;
            $scope.searchInfo.term = null;
            $scope.searchInfo.oldTerm = null;            
            $scope.searchInfo.results = [];
        }

	    //handles the on key up for searching, but we don't want to over query so the result is debounced
	    $scope.performSearch = _.debounce(function () {
	        angularHelper.safeApply($scope, function() {
	            if ($scope.searchInfo.term) {
	                if ($scope.searchInfo.oldTerm !== $scope.searchInfo.term) {
	                    $scope.searchInfo.results = [];

	                    var searchArgs = {
	                        term: $scope.searchInfo.term
	                    };
                        //append a start node id, whether it's a global one, or based on a selected list view
	                    if ($scope.searchInfo.searchFrom || dialogOptions.startNodeId) {
	                        searchArgs["searchFrom"] = $scope.searchInfo.searchFrom ? $scope.searchInfo.searchFrom : dialogOptions.startNodeId;
                        }
	                    searcher(searchArgs).then(function (data) {
	                        $scope.searchInfo.results = data;
	                        //now we need to look in the already selected search results and 
	                        // toggle the check boxes for those ones
	                        _.each($scope.searchInfo.results, function(result) {
	                            var exists = _.find($scope.searchInfo.selectedSearchResults, function (selectedId) {
	                                return result.id == selectedId;
	                            });
                                if (exists) {
                                    result.selected = true;
                                }
	                        });
                            
	                    });

	                    $scope.searchInfo.showSearch = true;
	                    $scope.searchInfo.oldTerm = $scope.searchInfo.term;
	                }
	            }
	        });
	        
	    }, 200);

	    $scope.dialogTreeEventHandler.bind("treeNodeSearch", nodeSearchHandler);
	    $scope.dialogTreeEventHandler.bind("treeNodeExpanded", nodeExpandedHandler);
	    $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);

	    $scope.$on('$destroy', function () {
	        $scope.dialogTreeEventHandler.unbind("treeNodeExpanded", nodeExpandedHandler);
	        $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
	        $scope.dialogTreeEventHandler.unbind("treeNodeSearch", nodeSearchHandler);
	    });
	});