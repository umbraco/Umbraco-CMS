//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Editors.TreePickerController",
    function ($scope,
        entityResource,
        eventsService,
        angularHelper,
        $timeout,
        localizationService,
        treeService,
        languageResource) {

        //used as the result selection
        $scope.model.selection = [];

        //the tree object when it loads
        var tree = null;

        // Search and listviews is only working for content, media and member section
        var searchableSections = ["content", "media", "member"];
        // tracks all expanded paths so when the language is switched we can resync it with the already loaded paths
        var expandedPaths = [];

        var vm = this;
        vm.treeReady = false;
        vm.dialogTreeApi = {};
        vm.initDialogTree = initDialogTree;
        vm.section = $scope.model.section;
        vm.treeAlias = $scope.model.treeAlias;
        vm.multiPicker = $scope.model.multiPicker;
        vm.hideHeader = (typeof $scope.model.hideHeader) === "boolean" ? $scope.model.hideHeader : true;
        vm.dataTypeKey = $scope.model.dataTypeKey;
        vm.searchInfo = {
            searchFromId: $scope.model.startNodeId,
            searchFromName: null,
            showSearch: false,
            dataTypeKey: vm.dataTypeKey,
            results: [],
            selectedSearchResults: []
        }
        vm.startNodeId = $scope.model.startNodeId;
        //Used for toggling an empty-state message
        //Some trees can have no items (dictionary & forms email templates)
        vm.hasItems = true;
        vm.emptyStateMessage = $scope.model.emptyStateMessage;
        vm.languages = [];
        vm.selectedLanguage = {};
        vm.languageSelectorIsOpen = false;
        vm.showLanguageSelector = $scope.model.showLanguageSelector;
        // Allow the entity type to be passed in but defaults to Document for backwards compatibility.
        vm.entityType = $scope.model.entityType ? $scope.model.entityType : "Document";
        vm.enableSearh = searchableSections.indexOf(vm.section) !== -1;


        vm.toggleLanguageSelector = toggleLanguageSelector;
        vm.selectLanguage = selectLanguage;
        vm.onSearchResults = onSearchResults;
        vm.selectResult = selectResult;
        vm.hideSearch = hideSearch;
        vm.closeMiniListView = closeMiniListView;
        vm.selectListViewNode = selectListViewNode;
        vm.listViewItemsLoaded = listViewItemsLoaded;
        vm.submit = submit;
        vm.close = close;

        var currentNode = $scope.model.currentNode;

        var previouslyFocusedElement = null;

        function initDialogTree() {
            vm.dialogTreeApi.callbacks.treeLoaded(treeLoadedHandler);
            // TODO: Also deal with unexpanding!!
            vm.dialogTreeApi.callbacks.treeNodeExpanded(nodeExpandedHandler);
            vm.dialogTreeApi.callbacks.treeNodeSelect(nodeSelectHandler);
        }

        /**
         * Performs the initialization of this component
         */
        function onInit() {

            if (vm.showLanguageSelector) {
                // load languages
                languageResource.getAll().then(function (languages) {
                    vm.languages = languages;

                    // set the default language
                    vm.languages.forEach(function (language) {
                        if (language.isDefault) {
                            vm.selectedLanguage = language;
                            vm.languageSelectorIsOpen = false;
                        }
                    });
                });
            }

            if (vm.treeAlias === "content") {
                vm.entityType = "Document";
                if (!$scope.model.title) {
                    localizationService.localize("defaultdialogs_selectContent").then(function (value) {
                        $scope.model.title = value;
                    });
                }
            }
            else if (vm.treeAlias === "documentTypes") {
                vm.entityType = "DocumentType";
                if (!$scope.model.title) {
                    localizationService.localize("defaultdialogs_selectContentType").then(function (value) {
                        $scope.model.title = value;
                    });
                }
            }
            else if (vm.treeAlias === "member" || vm.section === "member") {
                vm.entityType = "Member";
                if (!$scope.model.title) {
                    localizationService.localize("defaultdialogs_selectMember").then(function (value) {
                        $scope.model.title = value;
                    });
                }
            }
            else if (vm.treeAlias === "memberTypes") {
                vm.entityType = "MemberType";
                if (!$scope.model.title) {
                    localizationService.localize("defaultdialogs_selectMemberType").then(function (value) {
                        $scope.model.title = value;
                    });
                }
            }
            else if (vm.treeAlias === "media" || vm.section === "media") {
                vm.entityType = "Media";
                if (!$scope.model.title) {
                    localizationService.localize("defaultdialogs_selectMedia").then(function (value) {
                        $scope.model.title = value;
                    });
                }
            }
            else if (vm.treeAlias === "mediaTypes") {
                vm.entityType = "MediaType";
                if (!$scope.model.title) {
                    localizationService.localize("defaultdialogs_selectMediaType").then(function (value) {
                        $scope.model.title = value;
                    });
                }
            }
            else if (vm.treeAlias === "templates") {
                vm.entityType = "Template";
            }

            // TODO: Seems odd this logic is here, i don't think it needs to be and should just exist on the property editor using this
            if ($scope.model.minNumber) {
                $scope.model.minNumber = parseInt($scope.model.minNumber, 10);
            }
            if ($scope.model.maxNumber) {
                $scope.model.maxNumber = parseInt($scope.model.maxNumber, 10);
            }

            //if a alternative startnode is used, we need to check if it is a container
            if (vm.enableSearh &&
                vm.startNodeId &&
                vm.startNodeId !== -1 &&
                vm.startNodeId !== "-1") {
                entityResource.getById(vm.startNodeId, vm.entityType).then(function (node) {
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
            if ($scope.model.filter) {

                $scope.model.filterExclude = false;
                $scope.model.filterAdvanced = false;

                //used advanced filtering
                if (Utilities.isFunction($scope.model.filter)) {
                    $scope.model.filterAdvanced = true;
                }
                else if (Utilities.isObject($scope.model.filter)) {
                    $scope.model.filterAdvanced = true;
                }
                else {
                    if ($scope.model.filter.startsWith("!")) {
                        $scope.model.filterExclude = true;
                        $scope.model.filter = $scope.model.filter.substring(1);
                    }

                    //used advanced filtering
                    if ($scope.model.filter.startsWith("{")) {
                        $scope.model.filterAdvanced = true;

                        if ($scope.model.filterByMetadata && !Utilities.isFunction($scope.model.filter))
                        {
                            var filter = Utilities.fromJson($scope.model.filter);
                            $scope.model.filter = function (node){ return _.isMatch(node.metaData, filter);};
                        }            
                        else
                        {
                            //convert to object
                            $scope.model.filter = Utilities.fromJson($scope.model.filter);
                        }
                    }
                }
            }

            vm.filter = {
                filterAdvanced: $scope.model.filterAdvanced,
                filterExclude: $scope.model.filterExclude,
                filter: $scope.model.filter
            };
        }

        /**
         * Updates the tree's query parameters
         */
        function initTree() {
            //create the custom query string param for this tree
            var queryParams = {};
            if (vm.startNodeId) {
                queryParams["startNodeId"] = $scope.model.startNodeId;
            }
            if (vm.selectedLanguage && vm.selectedLanguage.id) {
                queryParams["culture"] = vm.selectedLanguage.culture;
            }
            if (vm.dataTypeKey) {
                queryParams["dataTypeKey"] = vm.dataTypeKey;
            }

            var queryString = $.param(queryParams); //create the query string from the params object

            if (!queryString) {
                vm.customTreeParams = $scope.model.customTreeParams;
            }
            else {
                vm.customTreeParams = queryString;
                if ($scope.model.customTreeParams) {
                    vm.customTreeParams += "&" + $scope.model.customTreeParams;
                }
            }

            vm.treeReady = true;
        }

        function selectLanguage(language) {
            vm.selectedLanguage = language;
            // close the language selector
            vm.languageSelectorIsOpen = false;

            initTree(); //this will reset the tree params and the tree directive will pick up the changes in a $watch

            //execute after next digest because the internal watch on the customtreeparams needs to be bound now that we've changed it
            $timeout(function () {
                //reload the tree with it's updated querystring args
                vm.dialogTreeApi.load(vm.section).then(function () {

                    //create the list of promises
                    var promises = [];
                    for (var i = 0; i < expandedPaths.length; i++) {
                        promises.push(vm.dialogTreeApi.syncTree({ path: expandedPaths[i], activate: false }));
                    }
                    //execute them sequentially
                    angularHelper.executeSequentialPromises(promises);
                });
            });
        };

        function toggleLanguageSelector() {
            vm.languageSelectorIsOpen = !vm.languageSelectorIsOpen;
        };

        function nodeExpandedHandler(args) {

            //store the reference to the expanded node path
            if (args.node) {
                treeService._trackExpandedPaths(args.node, expandedPaths);
            }

            // open mini list view for list views
            if (args.node.metaData.isContainer) {
                openMiniListView(args.node);
            }

            if (Utilities.isArray(args.children)) {

                //iterate children
                args.children.forEach(child => {
                    //now we need to look in the already selected search results and
                    // toggle the check boxes for those ones that are listed
                    var exists = vm.searchInfo.selectedSearchResults.find(selected => child.id === selected.id);
                    if (exists) {
                        child.selected = true;
                    }
                });

                //check filter
                performFiltering(args.children);
            }
        }

        //gets the tree object when it loads
        function treeLoadedHandler(args) {
            //args.tree contains children (args.tree.root.children)
            vm.hasItems = args.tree.root.children.length > 0;

            tree = args.tree;

            var nodeHasPath = currentNode && currentNode.path;
            var startNodeNotDefined = !vm.startNodeId;
            if (startNodeNotDefined && nodeHasPath) {
                vm.dialogTreeApi.syncTree({ path: currentNode.path, activate: true });
            }
        }

        //wires up selection
        function nodeSelectHandler(args) {
            args.event.preventDefault();
            args.event.stopPropagation();

            if (args.node.metaData.isSearchResult) {
                //check if the item selected was a search result from a list view

                //unselect
                select(args.node.name, args.node.id);

                //remove it from the list view children
                var listView = args.node.parent();
                listView.children = _.reject(listView.children,
                    function (child) {
                        return child.id == args.node.id;
                    });

                //remove it from the custom tracked search result list
                vm.searchInfo.selectedSearchResults = _.reject(vm.searchInfo.selectedSearchResults,
                    function (i) {
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
                    $scope.model.select(args.node);
                }
                else {
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

                if (vm.multiPicker) {
                    if (entity) {
                        multiSelectItem(entity);
                    }
                    else {
                        multiSelectItem(rootNode);
                    }
                }
                else {
                    $scope.model.selection.push(rootNode);
                    $scope.model.submit($scope.model);
                }
            }
            else {

                if (vm.multiPicker) {

                    if (entity) {
                        multiSelectItem(entity);
                    }
                    else {
                        //otherwise we have to get it from the server
                        entityResource.getById(id, vm.entityType).then(function (ent) {
                            multiSelectItem(ent);
                        });
                    }

                }
                else {

                    hideSearch();

                    //if an entity has been passed in, use it
                    if (entity) {
                        $scope.model.selection.push(entity);
                        $scope.model.submit($scope.model);
                    }
                    else {
                        //otherwise we have to get it from the server
                        entityResource.getById(id, vm.entityType).then(function (ent) {
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
                for (var i = 0; $scope.model.selection.length > i; i++) {
                    var selectedItem = $scope.model.selection[i];
                    if (selectedItem.id === parseInt(item.id)) {
                        found = true;
                        foundIndex = i;
                    }
                }
            }

            if (found) {
                $scope.model.selection.splice(foundIndex, 1);
            }
            else {
                $scope.model.selection.push(item);
            }

        }

        function performFiltering(nodes) {

            if (!$scope.model.filter) {
                return;
            }

            //remove any list view search nodes from being filtered since these are special nodes that always must
            // be allowed to be clicked on
            nodes = _.filter(nodes,
                function (n) {
                    return !Utilities.isObject(n.metaData.listViewNode);
                });

            if ($scope.model.filterAdvanced) {

                //filter either based on a method or an object
                var filtered = Utilities.isFunction($scope.model.filter)
                    ? _.filter(nodes, $scope.model.filter)
                    : _.where(nodes, $scope.model.filter);

                filtered.forEach(function (value) {
                        value.filtered = true;
                        if ($scope.model.filterCssClass) {
                            if (!value.cssClasses) {
                                value.cssClasses = [];
                            }
                            value.cssClasses.push($scope.model.filterCssClass);
                            value.title = $scope.model.filterTitle;
                        }
                    });
            }
            else {
                var a = $scope.model.filter.toLowerCase().replace(/\s/g, '').split(',');
                nodes.forEach(function (value) {

                        var found = a.indexOf(value.metaData.contentType.toLowerCase()) >= 0;

                        if (!$scope.model.filterExclude && !found || $scope.model.filterExclude && found) {
                            value.filtered = true;

                            if ($scope.model.filterCssClass) {
                                if (!value.cssClasses) {
                                    value.cssClasses = [];
                                }
                                value.cssClasses.push($scope.model.filterCssClass);
                                value.title = $scope.model.filterTitle;
                            }
                        }
                    });
            }
        }

        function openMiniListView(node) {
            previouslyFocusedElement = document.activeElement;
            vm.miniListView = node;
        }

        function multiSubmit(result) {
            entityResource.getByIds(result, vm.entityType).then(function (ents) {
                $scope.submit(ents);
            });
        }

        /** method to select a search result */
        function selectResult(evt, result) {

            if (result.filtered) {
                return;
            }

            result.selected = result.selected === true ? false : true;

            //since result = an entity, we'll pass it in so we don't have to go back to the server
            select(result.name, result.id, result);

            //add/remove to our custom tracked list of selected search results
            if (result.selected) {
                vm.searchInfo.selectedSearchResults.push(result);
            }
            else {
                vm.searchInfo.selectedSearchResults = _.reject(vm.searchInfo.selectedSearchResults,
                    function (i) {
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
        }

        function hideSearch() {

            //Traverse the entire displayed tree and update each node to sync with the selected search results
            if (tree) {

                //we need to ensure that any currently displayed nodes that get selected
                // from the search get updated to have a check box!
                function checkChildren(children) {
                    children.forEach(child => {
                        //check if the id is in the selection, if so ensure it's flagged as selected
                        var exists = vm.searchInfo.selectedSearchResults.find(selected => child.id === selected.id);
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
                            listView.children = _.reject(listView.children,
                                function (c) {
                                    return c.id == child.id;
                                });
                        }

                        //check if the current node is a list view and if so, check if there's any new results
                        // that need to be added as child nodes to it based on search results selected
                        if (child.metaData.isContainer) {

                            child.cssClasses = _.reject(child.cssClasses,
                                function (c) {
                                    return c === 'tree-node-slide-up-hide-active';
                                });

                            var listViewResults = vm.searchInfo.selectedSearchResults.filter(i => i.parentId === child.id);

                            listViewResults.forEach(item => {
                                if (!child.children) return;

                                var childExists = child.children.find(c => c.id === item.id);

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
                                        parent: () => parent                                        
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

            vm.searchInfo.showSearch = false;
            vm.searchInfo.searchFromId = vm.startNodeId;
            vm.searchInfo.searchFromName = null;
            vm.searchInfo.results = [];
        }

        function onSearchResults(results) {

            //filter all items - this will mark an item as filtered
            performFiltering(results);

            //now actually remove all filtered items so they are not even displayed
            results = results.filter(item => !item.filtered);          
            vm.searchInfo.results = results;

            //sync with the curr selected results
            vm.searchInfo.results.forEach(result => {
                var exists = $scope.model.selection.find(item => result.id === item.id);               
                if (exists) {
                    result.selected = true;
                }
            });

            vm.searchInfo.showSearch = true;
        }

        function selectListViewNode(node) {
            select(node.name, node.id);
            //toggle checked state
            node.selected = node.selected === true ? false : true;
        }

        function closeMiniListView() {
            vm.miniListView = undefined;
            if (previouslyFocusedElement) {
                $timeout(function () {
                    previouslyFocusedElement.focus();
                    previouslyFocusedElement = null;
                });
            }
        }

        function listViewItemsLoaded(items) {
            var selectedIds = $scope.model.selection.map(x => x.id);
            items.forEach(item => item.selected = selectedIds.includes(item.id));
        }

        function submit(model) {
            if ($scope.model.submit) {
                $scope.model.submit(model);
            }
        }

        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }

        //initialize
        onInit();

    });
