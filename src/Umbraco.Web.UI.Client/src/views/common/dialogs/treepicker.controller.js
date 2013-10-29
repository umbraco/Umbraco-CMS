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
	    $scope.entityType = "Document";
	    $scope.results = [];

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
	            $scope.showSearch = false;
	            $scope.results = [];
	            $scope.term = "";
	            $scope.oldTerm = undefined;

	            if ($scope.multiPicker) {
	                $scope.select(id);
	            }
	            else {
	                //if an entity has been passed in, use it
	                if (entity) {
	                    $scope.submit(entity);
	                }
	                else {
	                    //otherwise we have to get it from the server
	                    entityResource.getById(id, $scope.entityType).then(function (ent) {
	                        $scope.submit(ent);
	                    });
	                }
	            }
	        }
	    }

	    if (dialogOptions.section === "member") {
	        $scope.searcher = searchService.searchMembers;
	        $scope.entityType = "Member";
	    }
	    else if (dialogOptions.section === "media") {
	        $scope.searcher = searchService.searchMedia;
	        $scope.entityType = "Media";
	    }

	    $scope.multiSubmit = function (result) {
	        entityResource.getByIds(result, $scope.entityType).then(function (ents) {
	            $scope.submit(ents);
	        });
	    };

        /** method to select a search result */ 
	    $scope.selectResult = function (result) {
	        //since result = an entity, we'll pass it in so we don't have to go back to the server
	        select(result.name, result.id, result);
	    };

	    $scope.performSearch = function () {
	        if ($scope.term) {
	            if ($scope.oldTerm !== $scope.term) {
	                $scope.results = [];

	                $scope.searcher({ term: $scope.term }).then(function (data) {
	                    $scope.results = data;
	                });

	                $scope.showSearch = true;
	                $scope.oldTerm = $scope.term;
	            }
	        }
	        else {
	            $scope.oldTerm = "";
	            $scope.showSearch = false;
	            $scope.results = [];
	        }
	    };

	    if (dialogOptions.filter) {
	        var filterArray = dialogOptions.filter.split(',');

	        $scope.dialogTreeEventHandler.bind("treeNodeExpanded", function (ev, args) {

	            if (angular.isArray(args.children)) {
	                angular.forEach(args.children, function (value, key) {
	                    if (filterArray.indexOf(value.metaData.contentType) >= 0) {
	                        value.filtered = true;
	                        value.cssClasses.push("not-allowed");
	                    }
	                });
	            }

	        });
	    }

	    $scope.dialogTreeEventHandler.bind("treeNodeSelect", function (ev, args) {
	        args.event.preventDefault();
	        args.event.stopPropagation();

	        eventsService.publish("Umbraco.Dialogs.TreePickerController.Select", args).then(function (a) {

	            if (a.node.filtered) {
	                return;
	            }

	            //This is a tree node, so we don't have an entity to pass in, it will need to be looked up
	            //from the server in this method.
	            select(a.node.name, a.node.id);

	            //ui...
	            if ($scope.multiPicker) {
	                var c = $(a.event.target.parentElement);
	                if (!a.node.selected) {
	                    a.node.selected = true;
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
	                    a.node.selected = false;
	                    c.find(".temporary").remove();
	                    c.find("i.umb-tree-icon").show();
	                }
	            }
	        });
	    });
	});