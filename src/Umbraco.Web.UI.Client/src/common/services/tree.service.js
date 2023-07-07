
/**
 * @ngdoc service
 * @name umbraco.services.treeService
 * @function
 *
 * @description
 * The tree service factory, used internally by the umbTree and umbTreeItem directives
 */
function treeService($q, treeResource, iconHelper, notificationsService, eventsService) {

    //SD: Have looked at putting this in sessionStorage (not localStorage since that means you wouldn't be able to work
    // in multiple tabs) - however our tree structure is cyclical, meaning a node has a reference to it's parent and it's children
    // which you cannot serialize to sessionStorage. There's really no benefit of session storage except that you could refresh
    // a tab and have the trees where they used to be - supposed that is kind of nice but would mean we'd have to store the parent
    // as a nodeid reference instead of a variable with a getParent() method.
    var treeCache = {};

    var standardCssClass = 'icon umb-tree-icon sprTree';

    function getCacheKey(args) {
        //if there is no cache key they return null - it won't be cached.
        if (!args || !args.cacheKey) {
            return null;
        }

        var cacheKey = args.cacheKey;
        cacheKey += "_" + args.section;
        return cacheKey;
    }

    // Adapted from: https://stackoverflow.com/a/2140723
    // Please note, we can NOT test this functionality correctly in Phantom because it implements
    // the localeCompare method incorrectly: https://github.com/ariya/phantomjs/issues/11063
    function invariantEquals(a, b) {
        return typeof a === "string" && typeof b === "string"
            ? a.localeCompare(b, undefined, { sensitivity: "base" }) === 0
            : a === b;
    }

    return {

        /** Internal method to return the tree cache */
        _getTreeCache: function () {
            return treeCache;
        },

        /** Internal method to track expanded paths on a tree */
        _trackExpandedPaths: function (node, expandedPaths) {
            if (!node.children || !Utilities.isArray(node.children) || node.children.length == 0) {
                return;
            }

            //take the last child
            var childPath = this.getPath(node.children[node.children.length - 1]).join(",");
            //check if this already exists, if so exit
            if (expandedPaths.includes(childPath)) {
                return;
            }

            if (expandedPaths.length === 0) {
                expandedPaths.push(childPath); //track it
                return;
            }

            var clonedPaths = expandedPaths.slice(0); //make a copy to iterate over so we can modify the original in the iteration

            clonedPaths.forEach(p => {
                if (childPath.startsWith(p + ",")) {
                    //this means that the node's path supercedes this path stored so we can remove the current 'p' and replace it with node.path
                    expandedPaths.splice(expandedPaths.indexOf(p), 1); //remove it
                    if (expandedPaths.includes(childPath) === false) {
                        expandedPaths.push(childPath); //replace it
                    }
                }
                else if (p.startsWith(childPath + ",")) {
                    //this means we've already tracked a deeper node so we shouldn't track this one
                }
                else if (expandedPaths.includes(childPath) === false) {
                    expandedPaths.push(childPath); //track it
                }
            });
        },

        /** Internal method that ensures there's a routePath, parent and level property on each tree node and adds some icon specific properties so that the nodes display properly */
        _formatNodeDataForUseInUI: function (parentNode, treeNodes, section, level) {
            //if no level is set, then we make it 1
            var childLevel = (level ? level : 1);
            //set the section if it's not already set
            if (!parentNode.section) {
                parentNode.section = section;
            }

            if (parentNode.metaData && parentNode.metaData.noAccess === true) {
                if (!parentNode.cssClasses) {
                    parentNode.cssClasses = [];
                }
                parentNode.cssClasses.push("no-access");
            }

            //create a method outside of the loop to return the parent - otherwise jshint blows up
            var funcParent = function () {
                return parentNode;
            };

            for (var i = 0; i < treeNodes.length; i++) {

                var treeNode = treeNodes[i];

                treeNode.level = childLevel;

                //create a function to get the parent node, we could assign the parent node but
                // then we cannot serialize this entity because we have a cyclical reference.
                // Instead we just make a function to return the parentNode.
                treeNode.parent = funcParent;

                //set the section for each tree node - this allows us to reference this easily when accessing tree nodes
                treeNode.section = section;

                //if there is not route path specified, then set it automatically,
                //if this is a tree root node then we want to route to the section's dashboard
                if (!treeNode.routePath) {

                    if (treeNode.metaData && treeNode.metaData["treeAlias"]) {
                        //this is a root node
                        treeNode.routePath = section;
                    }
                    else {
                        var treeAlias = this.getTreeAlias(treeNode);
                        treeNode.routePath = section + "/" + treeAlias + "/edit/" + treeNode.id;
                    }
                }

                //now, format the icon data
                if (treeNode.iconIsClass === undefined || treeNode.iconIsClass) {
                    var converted = iconHelper.convertFromLegacyTreeNodeIcon(treeNode);
                    treeNode.cssClass = standardCssClass + " " + converted;
                    if (converted && converted.startsWith('.')) {
                        //its legacy so add some width/height
                        treeNode.style = "height:16px;width:16px;";
                    }
                    else {
                        treeNode.style = "";
                    }
                }
                else {
                    treeNode.style = "background-image: url('" + treeNode.iconFilePath + "');";
                    //we need an 'icon-' class in there for certain styles to work so if it is image based we'll add this
                    treeNode.cssClass = standardCssClass + " legacy-custom-file";
                }

                if (treeNode.metaData && treeNode.metaData.noAccess === true) {
                    if (!treeNode.cssClasses) {
                        treeNode.cssClasses = [];
                    }
                    treeNode.cssClasses.push("no-access");
                }
            }
        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#getTreePackageFolder
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Determines if the current tree is a plugin tree and if so returns the package folder it has declared
         * so we know where to find it's views, otherwise it will just return undefined.
         *
         * @param {String} treeAlias The tree alias to check
         */
        getTreePackageFolder: function (treeAlias) {
            //we determine this based on the server variables
            if (Umbraco.Sys.ServerVariables.umbracoPlugins &&
                Umbraco.Sys.ServerVariables.umbracoPlugins.trees &&
                Utilities.isArray(Umbraco.Sys.ServerVariables.umbracoPlugins.trees)) {

                var found = _.find(Umbraco.Sys.ServerVariables.umbracoPlugins.trees, function (item) {
                    return invariantEquals(item.alias, treeAlias);
                });

                return found ? found.packageFolder : undefined;
            }
            return undefined;
        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#clearCache
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Clears the tree cache - with optional cacheKey, optional section or optional filter.
         *
         * @param {Object} args arguments
         * @param {String} args.cacheKey optional cachekey - this is used to clear specific trees in dialogs
         * @param {String} args.section optional section alias - clear tree for a given section
         * @param {String} args.childrenOf optional parent ID - only clear the cache below a specific node
         */
        clearCache: function (args) {
            //clear all if not specified
            if (!args) {
                treeCache = {};
            }
            else {
                //if section and cache key specified just clear that cache
                if (args.section && args.cacheKey) {
                    var cacheKey = getCacheKey(args);
                    if (cacheKey && treeCache && treeCache[cacheKey] != null) {
                        treeCache = _.omit(treeCache, cacheKey);
                    }
                }
                else if (args.childrenOf) {
                    //if childrenOf is supplied a cacheKey must be supplied as well
                    if (!args.cacheKey) {
                        throw "args.cacheKey is required if args.childrenOf is supplied";
                    }
                    //this will clear out all children for the parentId passed in to this parameter, we'll
                    // do this by recursing and specifying a filter
                    var self = this;
                    this.clearCache({
                        cacheKey: args.cacheKey,
                        filter: function (cc) {
                            //get the new parent node from the tree cache
                            var parent = self.getDescendantNode(cc.root, args.childrenOf);
                            if (parent) {
                                //clear it's children and set to not expanded
                                parent.children = null;
                                parent.expanded = false;
                            }
                            //return the cache to be saved
                            return cc;
                        }
                    });
                }
                else if (args.filter && Utilities.isFunction(args.filter)) {
                    //if a filter is supplied a cacheKey must be supplied as well
                    if (!args.cacheKey) {
                        throw "args.cacheKey is required if args.filter is supplied";
                    }

                    //if a filter is supplied the function needs to return the data to keep
                    var byKey = treeCache[args.cacheKey];
                    if (byKey) {
                        var result = args.filter(byKey);

                        if (result) {
                            //set the result to the filtered data
                            treeCache[args.cacheKey] = result;
                        }
                        else {
                            //remove the cache
                            treeCache = _.omit(treeCache, args.cacheKey);
                        }

                    }

                }
                else if (args.cacheKey) {
                    //if only the cache key is specified, then clear all cache starting with that key
                    var allKeys1 = _.keys(treeCache);
                    var toRemove1 = _.filter(allKeys1, function (k) {
                        return k.startsWith(args.cacheKey + "_");
                    });
                    treeCache = _.omit(treeCache, toRemove1);
                }
                else if (args.section) {
                    //if only the section is specified then clear all cache regardless of cache key by that section
                    var allKeys2 = _.keys(treeCache);
                    var toRemove2 = _.filter(allKeys2, function (k) {
                        return k.endsWith("_" + args.section);
                    });
                    treeCache = _.omit(treeCache, toRemove2);
                }
            }
        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#loadNodeChildren
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Clears all node children, gets it's up-to-date children from the server and re-assigns them and then
         * returns them in a promise.
         * @param {object} args An arguments object
         * @param {object} args.node The tree node
         * @param {object} args.section The current section
         */
        loadNodeChildren: function (args) {
            if (!args) {
                throw "No args object defined for loadNodeChildren";
            }
            if (!args.node) {
                throw "No node defined on args object for loadNodeChildren";
            }

            // don't remove the children for container nodes in dialogs, as it'll remove the right arrow indicator
            if (!args.isDialog || !args.node.metaData.isContainer) {
                this.removeChildNodes(args.node);
            }
            args.node.loading = true;

            return this.getChildren(args)
                .then(function (data) {

                    //set state to done and expand (only if there actually are children!)
                    args.node.loading = false;
                    args.node.children = data;
                    if (args.node.children && args.node.children.length > 0) {
                        args.node.expanded = true;
                        args.node.hasChildren = true;

                        //Since we've removed the children &  reloaded them, we need to refresh the UI now because the tree node UI doesn't operate on normal angular $watch since that will be pretty slow
                        if (Utilities.isFunction(args.node.updateNodeData)) {
                            args.node.updateNodeData(args.node);
                        }
                    }

                    return $q.when(data);

                }, function (reason) {

                    //in case of error, emit event
                    eventsService.emit("treeService.treeNodeLoadError", { error: reason });

                    //stop show the loading indicator
                    args.node.loading = false;

                    //tell notications about the error
                    notificationsService.error(reason);

                    return $q.reject(reason);
                });

        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#removeNode
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Removes a given node from the tree
         * @param {object} treeNode the node to remove
         */
        removeNode: function (treeNode) {
            if (!Utilities.isFunction(treeNode.parent)) {
                return;
            }

            if (treeNode.parent() == null) {
                throw "Cannot remove a node that doesn't have a parent";
            }
            //remove the current item from it's siblings
            var parent = treeNode.parent();
            parent.children.splice(parent.children.indexOf(treeNode), 1);

            parent.hasChildren = parent.children.length !== 0;

            //Notify that the node has been removed
            eventsService.emit("treeService.removeNode", { node: treeNode });
        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#removeChildNodes
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Removes all child nodes from a given tree node
         * @param {object} treeNode the node to remove children from
         */
        removeChildNodes: function (treeNode) {
            treeNode.expanded = false;
            treeNode.children = [];
            treeNode.hasChildren = false;
        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#getChildNode
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Gets a child node with a given ID, from a specific treeNode
         * @param {object} treeNode to retrieve child node from
         * @param {int} id id of child node
         */
        getChildNode: function (treeNode, id) {
            if (!treeNode.children) {
                return null;
            }
            var found = _.find(treeNode.children, function (child) {
                return String(child.id).toLowerCase() === String(id).toLowerCase();
            });
            return found === undefined ? null : found;
        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#getDescendantNode
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Gets a descendant node by id
         * @param {object} treeNode to retrieve descendant node from
         * @param {int} id id of descendant node
         * @param {string} treeAlias - optional tree alias, if fetching descendant node from a child of a listview document
         */
        getDescendantNode: function (treeNode, id, treeAlias) {

            //validate if it is a section container since we'll need a treeAlias if it is one
            if (treeNode.isContainer === true && !treeAlias) {
                throw "Cannot get a descendant node from a section container node without a treeAlias specified";
            }

            //the treeNode passed in could be a section container, or it could be a section group
            //in either case we need to go through the children until we can find the actual tree root with the treeAlias
            var self = this;
            function getTreeRoot(tn) {
                //if it is a section container, we need to find the tree to be searched
                if (tn.isContainer) {
                    for (var c = 0; c < tn.children.length; c++) {
                        if (tn.children[c].isContainer) {
                            //recurse
                            var root = getTreeRoot(tn.children[c]);

                            //only return if we found the root in this child, otherwise continue.
                            if (root) {
                                return root;
                            }
                        }
                        else if (self.getTreeAlias(tn.children[c]) === treeAlias) {
                            return tn.children[c];
                        }
                    }
                    return null;
                }
                else {
                    return tn;
                }
            }

            var foundRoot = getTreeRoot(treeNode);
            if (!foundRoot) {
                throw "Could not find a tree in the current section with alias " + treeAlias;
            }
            treeNode = foundRoot;

            //check this node
            if (treeNode.id === id) {
                return treeNode;
            }

            //check the first level
            var found = this.getChildNode(treeNode, id);
            if (found) {
                return found;
            }

            //check each child of this node
            if (!treeNode.children) {
                return null;
            }

            for (var i = 0; i < treeNode.children.length; i++) {
                var child = treeNode.children[i];
                if (child.children && Utilities.isArray(child.children) && child.children.length > 0) {
                    //recurse
                    found = this.getDescendantNode(child, id);
                    if (found) {
                        return found;
                    }
                }
            }

            //not found
            return found === undefined ? null : found;
        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#getTreeRoot
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Gets the root node of the current tree type for a given tree node
         * @param {object} treeNode to retrieve tree root node from
         */
        getTreeRoot: function (treeNode) {
            if (!treeNode) {
                throw "treeNode cannot be null";
            }

            //all root nodes have metadata key 'treeAlias'
            var root = null;
            var current = treeNode;
            while (root === null && current) {

                if (current.metaData && current.metaData["treeAlias"]) {
                    root = current;
                }
                else if (Utilities.isFunction(current.parent)) {
                    //we can only continue if there is a parent() method which means this
                    // tree node was loaded in as part of a real tree, not just as a single tree
                    // node from the server.
                    current = current.parent();
                }
                else {
                    current = null;
                }
            }
            return root;
        },

        /** Gets the node's tree alias, this is done by looking up the meta-data of the current node's root node */
        /**
         * @ngdoc method
         * @name umbraco.services.treeService#getTreeAlias
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Gets the node's tree alias, this is done by looking up the meta-data of the current node's root node
         * @param {object} treeNode to retrieve tree alias from
         */
        getTreeAlias: function (treeNode) {
            var root = this.getTreeRoot(treeNode);
            if (root) {
                return root.metaData["treeAlias"];
            }
            return "";
        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#getTree
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * gets the tree, returns a promise
         * @param {object} args Arguments
         * @param {string} args.section Section alias
         * @param {string} args.cacheKey Optional cachekey
         */
        getTree: function (args) {

            //set defaults
            if (!args) {
                args = { section: 'content', cacheKey: null };
            }
            else if (!args.section) {
                args.section = 'content';
            }

            var cacheKey = getCacheKey(args);

            //return the cache if it exists
            if (cacheKey && treeCache[cacheKey] !== undefined) {
                return $q.when(treeCache[cacheKey]);
            }

            var self = this;
            return treeResource.loadApplication(args)
                .then(function (data) {
                    //this will be called once the tree app data has loaded
                    var result = {
                        name: data.name,
                        alias: args.section,
                        root: data
                    };

                    //format the root
                    self._formatNodeDataForUseInUI(result.root, result.root.children, args.section);

                    //if this is a root that contains group nodes, we need to format those manually too
                    if (result.root.containsGroups) {
                        for (var i = 0; i < result.root.children.length; i++) {
                            var group = result.root.children[i];

                            //we need to format/modify some of the node data to be used in our app.
                            self._formatNodeDataForUseInUI(group, group.children, args.section);
                        }
                    }

                    //cache this result if a cache key is specified - generally a cache key should ONLY
                    // be specified for application trees, dialog trees should not be cached.
                    if (cacheKey) {
                        treeCache[cacheKey] = result;
                        return $q.when(treeCache[cacheKey]);
                    }

                    //return un-cached
                    return $q.when(result);
                });
        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#getMenu
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Returns available menu actions for a given tree node
         * @param {object} args Arguments
         * @param {string} args.treeNode tree node object to retrieve the menu for
         */
        getMenu: function (args) {

            if (!args) {
                throw "args cannot be null";
            }
            if (!args.treeNode) {
                throw "args.treeNode cannot be null";
            }

            return treeResource.loadMenu(args.treeNode)
                .then(function (data) {
                    //need to convert the icons to new ones
                    for (var i = 0; i < data.length; i++) {
                        data[i].cssclass = iconHelper.convertFromLegacyIcon(data[i].cssclass);
                    }
                    return data;
                });
        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#getChildren
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Gets the children from the server for a given node
         * @param {object} args Arguments
         * @param {object} args.node tree node object to retrieve the children for
         * @param {string} args.section current section alias
         */
        getChildren: function (args) {

            if (!args) {
                throw "No args object defined for getChildren";
            }
            if (!args.node) {
                throw "No node defined on args object for getChildren";
            }

            var section = args.section || 'content';
            var treeItem = args.node;

            var self = this;

            return treeResource.loadNodes({ node: treeItem })
                .then(function (data) {
                    //now that we have the data, we need to add the level property to each item and the view
                    self._formatNodeDataForUseInUI(treeItem, data, section, treeItem.level + 1);
                    return $q.when(data);
                });
        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#reloadNode
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Re-loads the single node from the server
         * @param {object} node Tree node to reload
         */
        reloadNode: function (node) {
            if (!node) {
                throw "node cannot be null";
            }
            if (!node.parent()) {
                throw "cannot reload a single node without a parent";
            }
            if (!node.section) {
                throw "cannot reload a single node without an assigned node.section";
            }

            //set the node to loading
            node.loading = true;

            return this.getChildren({ node: node.parent(), section: node.section }).then(function (data) {

                //ok, now that we have the children, find the node we're reloading
                var found = _.find(data, function (item) {
                    return item.id === node.id;
                });
                if (found) {
                    //now we need to find the node in the parent.children collection to replace
                    var index = _.indexOf(node.parent().children, node);
                    //the trick here is to not actually replace the node - this would cause the delete animations
                    //to fire, instead we're just going to replace all the properties of this node.

                    //there should always be a method assigned but we'll check anyways
                    if (Utilities.isFunction(node.parent().children[index].updateNodeData)) {
                        node.parent().children[index].updateNodeData(found);
                    }
                    else {
                        //just update as per normal - this means styles, etc.. won't be applied
                        _.extend(node.parent().children[index], found);
                    }

                    //set the node loading
                    node.parent().children[index].loading = false;
                    //return
                    return $q.when(node.parent().children[index]);
                }
                else {
                    return $q.reject();
                }
            }, function () {
                return $q.reject();
            });
        },

        /**
         * @ngdoc method
         * @name umbraco.services.treeService#getPath
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * This will return the current node's path by walking up the tree
         * @param {object} node Tree node to retrieve path for
         */
        getPath: function (node) {
            if (!node) {
                throw "node cannot be null";
            }
            if (!Utilities.isFunction(node.parent)) {
                throw "node.parent is not a function, the path cannot be resolved";
            }

            var reversePath = [];
            var current = node;
            while (current != null) {
                reversePath.push(current.id);

                //all tree root nodes (non group, not section root) have a treeAlias so exit if that is the case
                //or exit if we cannot traverse further up
                if ((current.metaData && current.metaData["treeAlias"]) || !current.parent) {
                    current = null;
                }
                else {
                    current = current.parent();
                }
            }
            return reversePath.reverse();
        },

        syncTree: function (args) {

            if (!args) {
                throw "No args object defined for syncTree";
            }
            if (!args.node) {
                throw "No node defined on args object for syncTree";
            }
            if (!args.path) {
                throw "No path defined on args object for syncTree";
            }
            if (!Utilities.isArray(args.path)) {
                throw "Path must be an array";
            }
            if (args.path.length < 1) {
                //if there is no path, make -1 the path, and that should sync the tree root
                args.path.push("-1");
            }

            //get the rootNode for the current node, we'll sync based on that
            var root = this.getTreeRoot(args.node);
            if (!root) {
                throw "Could not get the root tree node based on the node passed in";
            }

            //now we want to loop through the ids in the path, first we'll check if the first part
            //of the path is the root node, otherwise we'll search it's children.
            var currPathIndex = 0;
            //if the first id is the root node and there's only one... then consider it synced
            if (String(args.path[currPathIndex]).toLowerCase() === String(args.node.id).toLowerCase()) {
                if (args.path.length === 1) {
                    //return the root
                    return $q.when(root);
                }
                else {
                    //move to the next path part and continue
                    currPathIndex = 1;
                }
            }

            var deferred = $q.defer();

            //now that we have the first id to lookup, we can start the process

            var self = this;
            var node = args.node;

            var doSync = function () {
                //check if it exists in the already loaded children
                var child = self.getChildNode(node, args.path[currPathIndex]);
                if (child) {
                    if (args.path.length === (currPathIndex + 1)) {
                        //woot! synced the node
                        if (!args.forceReload) {
                            return $q.when(child);
                        }
                        else {
                            //even though we've found the node if forceReload is specified
                            //we want to go update this single node from the server
                            return self.reloadNode(child);
                        }
                    }
                    else {
                        //now we need to recurse with the updated node/currPathIndex
                        currPathIndex++;
                        node = child;
                        //recurse
                        return doSync();
                    }
                }
                else {
                    //couldn't find it in the
                    return self.loadNodeChildren({ node: node, section: node.section }).then(function (children) {

                        //send back some progress to allow the caller to deal with expanded nodes
                        deferred.notify({ type: "treeNodeExpanded", node: node, children: children })

                        //ok, got the children, let's find it
                        var found = self.getChildNode(node, args.path[currPathIndex]);
                        if (found) {
                            if (args.path.length === (currPathIndex + 1)) {
                                //woot! synced the node
                                return $q.when(found);
                            }
                            else {
                                //now we need to recurse with the updated node/currPathIndex
                                currPathIndex++;
                                node = found;
                                //recurse
                                return doSync();
                            }
                        }
                        else {
                            //fail!
                            return $q.reject();
                        }
                    }, function () {
                        //fail!
                        return $q.reject();
                    });
                }
            };

            //start
            var wrappedPromise = doSync();

            //then wrap it 
            wrappedPromise.then(function (args) {
                deferred.resolve(args);
            }, function (args) {
                deferred.reject(args);
            });

            return deferred.promise;
        }

    };
}

angular.module('umbraco.services').factory('treeService', treeService);
