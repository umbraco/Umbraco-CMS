//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Overlays.TreePickerController",
    function ($scope, $q, entityResource, eventsService, $log, searchService, angularHelper, $timeout, localizationService, treeService, contentResource, mediaResource, memberResource) {

        var tree = null;
        var dialogOptions = $scope.model;
        $scope.treeReady = false;
        $scope.dialogTreeEventHandler = $({});
        $scope.section = dialogOptions.section;
        $scope.treeAlias = dialogOptions.treeAlias;
        $scope.multiPicker = dialogOptions.multiPicker;
        $scope.hideHeader = (typeof dialogOptions.hideHeader) === "boolean" ? dialogOptions.hideHeader : true;
        // if you need to load a not initialized tree set this value to false - default is true
        $scope.onlyInitialized = dialogOptions.onlyInitialized;
        $scope.searchInfo = {
            searchFromId: dialogOptions.startNodeId,
            searchFromName: null,
            showSearch: false,
            ignoreUserStartNodes: dialogOptions.ignoreUserStartNodes,
            results: [],
            selectedSearchResults: []
        }

        $scope.model.selection = [];

        //Used for toggling an empty-state message
        //Some trees can have no items (dictionary & forms email templates)
        $scope.hasItems = true;
        $scope.emptyStateMessage = dialogOptions.emptyStateMessage;
        var node = dialogOptions.currentNode;

        //This is called from ng-init
        //it turns out it is called from the angular html : / Have a look at views/common / overlays / contentpicker / contentpicker.html you'll see ng-init.
        //this is probably an anti pattern IMO and shouldn't be used
        $scope.init = function (contentType) {

            if (contentType === "content") {
                $scope.entityType = "Document";
                if (!$scope.model.title) {
                    $scope.model.title = localizationService.localize("defaultdialogs_selectContent");
                }
            } else if (contentType === "member") {
                $scope.entityType = "Member";
                if (!$scope.model.title) {
                    $scope.model.title = localizationService.localize("defaultdialogs_selectMember");
                }
            } else if (contentType === "media") {
                $scope.entityType = "Media";
                if (!$scope.model.title) {
                    $scope.model.title = localizationService.localize("defaultdialogs_selectMedia");
                }
            }
        }

        var searchText = "Search...";
        localizationService.localize("general_search").then(function (value) {
            searchText = value + "...";
        });

        // Allow the entity type to be passed in but defaults to Document for backwards compatibility.
        $scope.entityType = dialogOptions.entityType ? dialogOptions.entityType : "Document";


        //min / max values
        if (dialogOptions.minNumber) {
            dialogOptions.minNumber = parseInt(dialogOptions.minNumber, 10);
        }
        if (dialogOptions.maxNumber) {
            dialogOptions.maxNumber = parseInt(dialogOptions.maxNumber, 10);
        }

        if (dialogOptions.section === "member") {
            $scope.entityType = "Member";
        }
        else if (dialogOptions.section === "media") {
            $scope.entityType = "Media";
        }

        // Search and listviews is only working for content, media and member section
        var searchableSections = ["content", "media", "member"];

        $scope.enableSearh = searchableSections.indexOf($scope.section) !== -1;

        //if a alternative startnode is used, we need to check if it is a container
        if ($scope.enableSearh && dialogOptions.startNodeId && dialogOptions.startNodeId !== -1 && dialogOptions.startNodeId !== "-1") {
            entityResource.getById(dialogOptions.startNodeId, $scope.entityType).then(function(node) {
                if (node.metaData.IsContainer) {
                    openMiniListView(node);
                }
                initTree();
            });
        }
        else {
            initTree();
        }

        //Configures filtering
        if (dialogOptions.filter) {

            dialogOptions.filterExclude = false;
            dialogOptions.filterAdvanced = false;

            //used advanced filtering
            if (angular.isFunction(dialogOptions.filter)) {
                dialogOptions.filterAdvanced = true;
            }
            else if (angular.isObject(dialogOptions.filter)) {
                dialogOptions.filterAdvanced = true;
            }
            else {
                if (dialogOptions.filter.startsWith("!")) {
                    dialogOptions.filterExclude = true;
                    dialogOptions.filterTypes = dialogOptions.filter.substring(1);
                } else {
                    dialogOptions.filterExclude = false;
                    dialogOptions.filterTypes = dialogOptions.filter;
                }

                //used advanced filtering
                if (dialogOptions.filter.startsWith("{")) {
                    dialogOptions.filterAdvanced = true;
                    //convert to object
                    dialogOptions.filter = angular.fromJson(dialogOptions.filter);
                }
            }

            $scope.filter = {
                filterAdvanced: dialogOptions.filterAdvanced,
                filterExclude: dialogOptions.filterExclude,
                filter: dialogOptions.filterTypes
            };
        }

        function initTree() {
            //create the custom query string param for this tree
            var params = [];

            if (dialogOptions.startNodeId)
                params.push("startNodeId=" + dialogOptions.startNodeId);

            if (dialogOptions.ignoreUserStartNodes)
                params.push("ignoreUserStartNodes=" + dialogOptions.ignoreUserStartNodes);

            if (dialogOptions.customTreeParams)
                params.push(dialogOptions.customTreeParams);

            $scope.customTreeParams = params.join('&');

            $scope.treeReady = true;
        }

        function nodeExpandedHandler(ev, args) {

            // open mini list view for list views
            if (args.node.metaData.isContainer) {
                openMiniListView(args.node);
            }

            if (angular.isArray(args.children)) {

                //iterate children
                _.each(args.children, function (child) {

                    //now we need to look in the already selected search results and
                    // toggle the check boxes for those ones that are listed
                    var exists = _.find($scope.searchInfo.selectedSearchResults, function (selected) {
                        return child.id == selected.id;
                    });
                    if (exists) {
                        child.selected = true;
                    }
                });

                //check filter
                performFiltering(args.children);
            }
        }

        //gets the tree object when it loads
        function treeLoadedHandler(ev, args) {
            //args.tree contains children (args.tree.root.children)
            $scope.hasItems = args.tree.root.children.length > 0;

            tree = args.tree;

            var nodeHasPath = typeof node !== "undefined" && typeof node.path !== "undefined";
            var startNodeNotDefined = typeof dialogOptions.startNodeId === "undefined" || dialogOptions.startNodeId === "" || dialogOptions.startNodeId === "-1";
            if (startNodeNotDefined && nodeHasPath) {
                $scope.dialogTreeEventHandler.syncTree({ path: node.path, activate: false });
            }

        }

        //wires up selection
        function nodeSelectHandler(ev, args) {
            args.event.preventDefault();
            args.event.stopPropagation();

            if (args.node.metaData.isSearchResult) {
                //check if the item selected was a search result from a list view

                //unselect
                select(args.node.name, args.node.id);

                //remove it from the list view children
                var listView = args.node.parent();
                listView.children = _.reject(listView.children, function (child) {
                    return child.id == args.node.id;
                });

                //remove it from the custom tracked search result list
                $scope.searchInfo.selectedSearchResults = _.reject($scope.searchInfo.selectedSearchResults, function (i) {
                    return i.id == args.node.id;
                });
            }
            else {
                eventsService.emit("dialogs.treePickerController.select", args);

                if (args.node.filtered) {
                    return;
                }

                //This is a tree node, so we don't have an entity to pass in, it will need to be looked up
                //from the server in this method.
                if ($scope.model.select) {
                    $scope.model.select(args.node)
                } else {
                    select(args.node.name, args.node.id);
                    //toggle checked state
                    args.node.selected = args.node.selected === true ? false : true;
                }

            }
        }

        /** Method used for selecting a node */
        function select(text, id, entity) {
            //if we get the root, we just return a constructed entity, no need for server data
            if (id < 0) {

                var rootNode = {
                    alias: null,
                    icon: "icon-folder",
                    id: id,
                    name: text
                };

                if ($scope.multiPicker) {
                    if (entity) {
                        multiSelectItem(entity);
                    } else {
                        multiSelectItem(rootNode);
                    }
                }
                else {
                    $scope.model.selection.push(rootNode);
                    $scope.model.submit($scope.model);
                }
            }
            else {

                if ($scope.multiPicker) {

                    if (entity) {
                        multiSelectItem(entity);
                    } else {
                        //otherwise we have to get it from the server
                        entityResource.getById(id, $scope.entityType).then(function (ent) {
                            multiSelectItem(ent);
                        });
                    }

                }

                else {

                    $scope.hideSearch();

                    //if an entity has been passed in, use it
                    if (entity) {
                        $scope.model.selection.push(entity);
                        $scope.model.submit($scope.model);
                    } else {
                        //otherwise we have to get it from the server
                        entityResource.getById(id, $scope.entityType).then(function (ent) {
                            $scope.model.selection.push(ent);
                            $scope.model.submit($scope.model);
                        });
                    }
                }
            }
        }

        function multiSelectItem(item) {

            var found = false;
            var foundIndex = 0;

            if ($scope.model.selection.length > 0) {
                for (i = 0; $scope.model.selection.length > i; i++) {
                    var selectedItem = $scope.model.selection[i];
                    if (selectedItem.id === item.id) {
                        found = true;
                        foundIndex = i;
                    }
                }
            }

            if (found) {
                $scope.model.selection.splice(foundIndex, 1);
            } else {
                $scope.model.selection.push(item);
            }

        }

        function performFiltering(nodes) {

            if (!dialogOptions.filter) {
                return;
            }

            //remove any list view search nodes from being filtered since these are special nodes that always must
            // be allowed to be clicked on
            nodes = _.filter(nodes, function (n) {
                return !angular.isObject(n.metaData.listViewNode);
            });

            if (dialogOptions.filterAdvanced) {

                //filter either based on a method or an object
                var filtered = angular.isFunction(dialogOptions.filter)
                    ? _.filter(nodes, dialogOptions.filter)
                    : _.where(nodes, dialogOptions.filter);

                angular.forEach(filtered, function (value, key) {
                    value.filtered = true;
                    if (dialogOptions.filterCssClass) {
                        if (!value.cssClasses) {
                            value.cssClasses = [];
                        }
                        value.cssClasses.push(dialogOptions.filterCssClass);
                    }
                });
            } else {
                var a = dialogOptions.filterTypes.toLowerCase().replace(/\s/g, '').split(',');
                angular.forEach(nodes, function (value, key) {

                    var found = a.indexOf(value.metaData.contentType.toLowerCase()) >= 0;

                    if (!dialogOptions.filterExclude && !found || dialogOptions.filterExclude && found) {
                        value.filtered = true;

                        if (dialogOptions.filterCssClass) {
                            if (!value.cssClasses) {
                                value.cssClasses = [];
                            }
                            value.cssClasses.push(dialogOptions.filterCssClass);
                        }
                    }
                });
            }
        }

        $scope.multiSubmit = function (result) {
            entityResource.getByIds(result, $scope.entityType).then(function (ents) {
                $scope.submit(ents);
            });
        };

        /** method to select a search result */
        $scope.selectResult = function (evt, result) {

            if (result.filtered) {
                return;
            }

            result.selected = result.selected === true ? false : true;

            //since result = an entity, we'll pass it in so we don't have to go back to the server
            select(result.name, result.id, result);

            //add/remove to our custom tracked list of selected search results
            if (result.selected) {
                $scope.searchInfo.selectedSearchResults.push(result);
            }
            else {
                $scope.searchInfo.selectedSearchResults = _.reject($scope.searchInfo.selectedSearchResults, function (i) {
                    return i.id == result.id;
                });
            }

            //ensure the tree node in the tree is checked/unchecked if it already exists there
            if (tree) {
                var found = treeService.getDescendantNode(tree.root, result.id);
                if (found) {
                    found.selected = result.selected;
                }
            }

        };

        $scope.hideSearch = function () {

            //Traverse the entire displayed tree and update each node to sync with the selected search results
            if (tree) {

                //we need to ensure that any currently displayed nodes that get selected
                // from the search get updated to have a check box!
                function checkChildren(children) {
                    _.each(children, function (child) {
                        //check if the id is in the selection, if so ensure it's flagged as selected
                        var exists = _.find($scope.searchInfo.selectedSearchResults, function (selected) {
                            return child.id == selected.id;
                        });
                        //if the curr node exists in selected search results, ensure it's checked
                        if (exists) {
                            child.selected = true;
                        }
                        //if the curr node does not exist in the selected search result, and the curr node is a child of a list view search result
                        else if (child.metaData.isSearchResult) {
                            //if this tree node is under a list view it means that the node was added
                            // to the tree dynamically under the list view that was searched, so we actually want to remove
                            // it all together from the tree
                            var listView = child.parent();
                            listView.children = _.reject(listView.children, function (c) {
                                return c.id == child.id;
                            });
                        }

                        //check if the current node is a list view and if so, check if there's any new results
                        // that need to be added as child nodes to it based on search results selected
                        if (child.metaData.isContainer) {

                            child.cssClasses = _.reject(child.cssClasses, function (c) {
                                return c === 'tree-node-slide-up-hide-active';
                            });

                            var listViewResults = _.filter($scope.searchInfo.selectedSearchResults, function (i) {
                                return i.parentId == child.id;
                            });
                            _.each(listViewResults, function (item) {
                                var childExists = _.find(child.children, function (c) {
                                    return c.id == item.id;
                                });
                                if (!childExists) {
                                    var parent = child;
                                    child.children.unshift({
                                        id: item.id,
                                        name: item.name,
                                        cssClass: "icon umb-tree-icon sprTree " + item.icon,
                                        level: child.level + 1,
                                        metaData: {
                                            isSearchResult: true
                                        },
                                        hasChildren: false,
                                        parent: function () {
                                            return parent;
                                        }
                                    });
                                }
                            });
                        }

                        //recurse
                        if (child.children && child.children.length > 0) {
                            checkChildren(child.children);
                        }
                    });
                }
                checkChildren(tree.root.children);
            }


            $scope.searchInfo.showSearch = false;
            $scope.searchInfo.searchFromId = dialogOptions.startNodeId;
            $scope.searchInfo.searchFromName = null;
            $scope.searchInfo.results = [];
        }

        $scope.onSearchResults = function (results) {

            //filter all items - this will mark an item as filtered
            performFiltering(results);

            //now actually remove all filtered items so they are not even displayed
            results = _.filter(results, function (item) {
                return !item.filtered;
            });

            $scope.searchInfo.results = results;

            //sync with the curr selected results
            _.each($scope.searchInfo.results, function (result) {
                var exists = _.find($scope.model.selection, function (selectedId) {
                    return result.id == selectedId;
                });
                if (exists) {
                    result.selected = true;
                }
            });

            $scope.searchInfo.showSearch = true;
        };

        $scope.dialogTreeEventHandler.bind("treeLoaded", treeLoadedHandler);
        $scope.dialogTreeEventHandler.bind("treeNodeExpanded", nodeExpandedHandler);
        $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);

        $scope.$on('$destroy', function () {
            $scope.dialogTreeEventHandler.unbind("treeLoaded", treeLoadedHandler);
            $scope.dialogTreeEventHandler.unbind("treeNodeExpanded", nodeExpandedHandler);
            $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
        });

        $scope.selectListViewNode = function (node) {
            select(node.name, node.id);
            //toggle checked state
            node.selected = node.selected === true ? false : true;
        };

        $scope.closeMiniListView = function () {
            $scope.miniListView = undefined;
        };

        function openMiniListView(node) {
            $scope.miniListView = node;
        }

    });
