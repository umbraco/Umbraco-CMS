
/**
 * @ngdoc service
 * @name umbraco.services.treeService
 * @function
 *
 * @description
 * The tree service factory, used internally by the umbTree and umbTreeItem directives
 */
function treeService($q, treeResource, iconHelper, notificationsService, $rootScope, umbSessionStorage) {

    //initialize the tree cache if nothing is there, we store the cache in sessionStorage which 
    // is applicable to the current open tab
    if (!umbSessionStorage.get("treeCache")) {
        umbSessionStorage.set("treeCache", {});
    }
    
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

    return {  

        /** Internal method to return the tree cache - this also wires up the parent() function for each node since when we serialize to cache, functions are obviously lost */
        _getTreeCache: function(key) {
            var cache = umbSessionStorage.get("treeCache");
            
            //method to set the parent() delegate for each node
            var setParent = function (currParent, currChildren) {
                _.each(currChildren, function (child, index) {
                    //create the method, return it's parent
                    child.parent = function () {
                        return currParent;
                    };
                    if (angular.isArray(child.children) && child.children.length > 0) {
                        //recurse
                        setParent(child, child.children);
                    }

                });
            };

            //return the raw cache if a key is specified but there is nothing to process with that key
            if (key && (!cache[key] || !cache[key].root || !angular.isArray(cache[key].root.children))) {
                return cache;
            }
            else if (key && cache[key]) {
                //if a key is specified only process that portion of the cache
                setParent(cache[key].root, cache[key].root.children);
                return cache;
            }
            else {
                //no key is specified, process all of the cache
                _.each(cache, function (val) {
                    if (val.root && angular.isArray(val.root.children)) {                        
                        setParent(val.root, val.root.children);
                    }
                });
                return cache;
            }
            
        },

        /** Internal method that ensures there's a routePath, parent and level property on each tree node and adds some icon specific properties so that the nodes display properly */
        _formatNodeDataForUseInUI: function (parentNode, treeNodes, section, level) {
            //if no level is set, then we make it 1   
            var childLevel = (level ? level : 1);
            //set the section if it's not already set
            if (!parentNode.section) {
                parentNode.section = section;
            }
            //create a method outside of the loop to return the parent - otherwise jshint blows up
            var funcParent = function() {
                return parentNode;
            };
            for (var i = 0; i < treeNodes.length; i++) {

                treeNodes[i].level = childLevel;

                //create a function to get the parent node, we could assign the parent node but 
                // then we cannot serialize this entity because we have a cyclical reference.
                // Instead we just make a function to return the parentNode.
                treeNodes[i].parent = funcParent;

                //set the section for each tree node - this allows us to reference this easily when accessing tree nodes
                treeNodes[i].section = section;

                //if there is not route path specified, then set it automatically,
                //if this is a tree root node then we want to route to the section's dashboard
                if (!treeNodes[i].routePath) {
                    
                    if (treeNodes[i].metaData && treeNodes[i].metaData["treeAlias"]) {
                        //this is a root node
                        treeNodes[i].routePath = section;                        
                    }
                    else {
                        var treeAlias = this.getTreeAlias(treeNodes[i]);
                        treeNodes[i].routePath = section + "/" + treeAlias + "/edit/" + treeNodes[i].id;
                    }
                }

                //now, format the icon data
                if (treeNodes[i].iconIsClass === undefined || treeNodes[i].iconIsClass) {
                    var converted = iconHelper.convertFromLegacyTreeNodeIcon(treeNodes[i]);
                    treeNodes[i].cssClass = standardCssClass + " " + converted;
                    if (converted.startsWith('.')) {
                        //its legacy so add some width/height
                        treeNodes[i].style = "height:16px;width:16px;";
                    }
                    else {
                        treeNodes[i].style = "";
                    }
                }
                else {
                    treeNodes[i].style = "background-image: url('" + treeNodes[i].iconFilePath + "');";
                    //we need an 'icon-' class in there for certain styles to work so if it is image based we'll add this
                    treeNodes[i].cssClass = standardCssClass + " legacy-custom-file";
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
        getTreePackageFolder: function(treeAlias) {            
            //we determine this based on the server variables
            if (Umbraco.Sys.ServerVariables.umbracoPlugins &&
                Umbraco.Sys.ServerVariables.umbracoPlugins.trees &&
                angular.isArray(Umbraco.Sys.ServerVariables.umbracoPlugins.trees)) {

                var found = _.find(Umbraco.Sys.ServerVariables.umbracoPlugins.trees, function(item) {
                    return item.alias === treeAlias;
                });
                
                return found ? found.packageFolder : undefined;
            }
            return undefined;
        },

        /** This puts the tree into cache */
        cacheTree: function (cacheKey, section, tree) {
            if (!cacheKey || !section || !tree) {
                return;
            }

            var key = getCacheKey({ cacheKey: cacheKey, section: section });
            //NOTE: we're not using the _getTreeCache method here because we don't want to process the parent() method, 
            // simply to get the raw values so we can update it.
            var rawCache = umbSessionStorage.get("treeCache");
            rawCache[key] = tree;
            umbSessionStorage.set("treeCache", rawCache);
        },

        /** clears the tree cache - with optional cacheKey, optional section or optional filter */
        clearCache: function (args) {
            //clear all if not specified
            if (!args) {
                umbSessionStorage.set("treeCache", {});
            }
            else {

                var treeCache = this._getTreeCache();

                //if section and cache key specified just clear that cache
                if (args.section && args.cacheKey) {
                    var cacheKey = getCacheKey(args);                    
                    if (cacheKey && treeCache && treeCache[cacheKey] != null) {
                        umbSessionStorage.set("treeCache", _.omit(treeCache, cacheKey));
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
                        filter: function(cc) {
                            //get the new parent node from the tree cache
                            var parent = self.getDescendantNode(cc.root, args.childrenOf);                            
                            //clear it's children and set to not expanded
                            parent.children = null;
                            parent.expanded = false;
                            //return the cache to be saved
                            return cc;
                        }
                    });
                }
                else if (args.filter && angular.isFunction(args.filter)) {
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
                            umbSessionStorage.set("treeCache", treeCache);
                        }
                        else {                            
                            //remove the cache
                            umbSessionStorage.set("treeCache", _.omit(treeCache, args.cacheKey));
                        }
                    }

                }
                else if (args.cacheKey) {
                    //if only the cache key is specified, then clear all cache starting with that key
                    var allKeys1 = _.keys(treeCache);
                    var toRemove1 = _.filter(allKeys1, function (k) {
                        return k.startsWith(args.cacheKey + "_");
                    });
                    umbSessionStorage.set("treeCache", _.omit(treeCache, toRemove1));
                }
                else if (args.section) {
                    //if only the section is specified then clear all cache regardless of cache key by that section
                    var allKeys2 = _.keys(treeCache);
                    var toRemove2 = _.filter(allKeys2, function (k) {
                        return k.endsWith("_" + args.section);
                    });
                    umbSessionStorage.set("treeCache", _.omit(treeCache, toRemove2));
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
        loadNodeChildren: function(args) {
            if (!args) {
                throw "No args object defined for loadNodeChildren";
            }
            if (!args.node) {
                throw "No node defined on args object for loadNodeChildren";
            }
            
            this.removeChildNodes(args.node);
            args.node.loading = true;

            return this.getChildren(args)
                .then(function(data) {

                    //set state to done and expand
                    args.node.loading = false;
                    args.node.children = data;
                    args.node.expanded = true;
                    args.node.hasChildren = true;

                    return data;

                }, function(reason) {

                    //in case of error, emit event
                    $rootScope.$broadcast("treeNodeLoadError", {error: reason });

                    //stop show the loading indicator  
                    node.loading = false;

                    //tell notications about the error
                    notificationsService.error(reason);

                    return reason;
                });

        },

        /** Removes a given tree node from the tree */
        removeNode: function(treeNode) {
            if (treeNode.parent() == null) {
                throw "Cannot remove a node that doesn't have a parent";
            }
            //remove the current item from it's siblings
            treeNode.parent().children.splice(treeNode.parent().children.indexOf(treeNode), 1);            
        },
        
        /** Removes all child nodes from a given tree node */
        removeChildNodes : function(treeNode) {
            treeNode.expanded = false;
            treeNode.children = [];
            treeNode.hasChildren = false;
        },

        /** Gets a child node by id */
        getChildNode: function (treeNode, id) {
            if (!treeNode.children) {
                return null;
            }
            var found = _.find(treeNode.children, function (child) {
                return String(child.id) === String(id);
            });
            return found === undefined ? null : found;
        },

        /** Gets a descendant node by id */
        getDescendantNode: function(treeNode, id) {
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
                if (treeNode.children[i].children && angular.isArray(treeNode.children[i].children) && treeNode.children[i].children.length > 0) {
                    //recurse
                    found = this.getDescendantNode(treeNode.children[i], id);
                    if (found) {
                        return found;
                    }
                }
            }
            
            //not found
            return found === undefined ? null : found;
        },

        /** Gets the root node of the current tree type for a given tree node */
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
                else { 
                    current = current.parent();
                }
            }
            return root;
        },

        /** Gets the node's tree alias, this is done by looking up the meta-data of the current node's root node */
        getTreeAlias : function(treeNode) {
            var root = this.getTreeRoot(treeNode);
            if (root) {
                return root.metaData["treeAlias"];
            }
            return "";
        },

        /** gets the tree, returns a promise */
        getTree: function (args) {

            var deferred = $q.defer();

            //set defaults
            if (!args) {
                args = { section: 'content', cacheKey: null };
            }
            else if (!args.section) {
                args.section = 'content';
            }

            var cacheKey = getCacheKey(args);
            var treeCache = this._getTreeCache(cacheKey);

            //return the cache if it exists
            if (cacheKey && treeCache[cacheKey] !== undefined) {
                deferred.resolve(treeCache[cacheKey]);
                return deferred.promise;
            }

            var self = this;
            treeResource.loadApplication(args)
                .then(function(data) {
                    //this will be called once the tree app data has loaded
                    var result = {
                        name: data.name,
                        alias: args.section,
                        root: data
                    };
                    //we need to format/modify some of the node data to be used in our app.
                    self._formatNodeDataForUseInUI(result.root, result.root.children, args.section);

                    ////cache this result if a cache key is specified - generally a cache key should ONLY
                    //// be specified for application trees, dialog trees should not be cached.
                    //if (cacheKey) {                        
                    //    treeCache[cacheKey] = result;
                    //    umbSessionStorage.set("treeCache", treeCache);
                    //    deferred.resolve(treeCache[cacheKey]);
                    //}

                    //return un-cached
                    deferred.resolve(result);
                });
            
            return deferred.promise;
        },

        getMenu: function (args) {

            if (!args) {
                throw "args cannot be null";
            }
            if (!args.treeNode) {
                throw "args.treeNode cannot be null";
            }

            return treeResource.loadMenu(args.treeNode)
                .then(function(data) {
                    //need to convert the icons to new ones
                    for (var i = 0; i < data.length; i++) {
                        data[i].cssclass = iconHelper.convertFromLegacyIcon(data[i].cssclass);
                    }
                    return data;
                });
        },
        
        /** Gets the children from the server for a given node */
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
                    return data;
                });
        },
        
        /** This re-loads the single node from the server */
        reloadNode: function(node) {
            if (!node) {
                throw "node cannot be null";
            }
            if (!node.parent()) {
                throw "cannot reload a single node without a parent";
            }
            if (!node.section) {
                throw "cannot reload a single node without an assigned node.section";
            }
            
            var deferred = $q.defer();
            
            //set the node to loading
            node.loading = true;

            this.getChildren({ node: node.parent(), section: node.section }).then(function(data) {

                //ok, now that we have the children, find the node we're reloading
                var found = _.find(data, function(item) {
                    return item.id === node.id;
                });
                if (found) {
                    //now we need to find the node in the parent.children collection to replace
                    var index = _.indexOf(node.parent().children, node);
                    //the trick here is to not actually replace the node - this would cause the delete animations
                    //to fire, instead we're just going to replace all the properties of this node.
                    _.extend(node.parent().children[index], found);
                    //set the node to loading
                    node.parent().children[index].loading = false;
                    //return
                    deferred.resolve(node.parent().children[index]);
                }
                else {
                    deferred.reject();
                }
            }, function() {
                deferred.reject();
            });
            
            return deferred.promise;
        },

        syncTree: function(args) {
            
            if (!args) {
                throw "No args object defined for syncTree";
            }
            if (!args.node) {
                throw "No node defined on args object for syncTree";
            }
            if (!args.path) {
                throw "No path defined on args object for syncTree";
            }
            if (!angular.isArray(args.path)) {
                throw "Path must be an array";
            }
            if (args.path.length < 1) {
                throw "args.path must contain at least one id";
            }

            var deferred = $q.defer();

            //get the rootNode for the current node, we'll sync based on that
            var root = this.getTreeRoot(args.node);
            if (!root) {
                throw "Could not get the root tree node based on the node passed in";
            }
            
            //now we want to loop through the ids in the path, first we'll check if the first part
            //of the path is the root node, otherwise we'll search it's children.
            var currPathIndex = 0;
            //if the first id is the root node and there's only one... then consider it synced
            if (String(args.path[currPathIndex]) === String(args.node.id)) {

                if (args.path.length === 1) {
                    //return the root
                    deferred.resolve(root);
                }
                else {
                    currPathIndex = 1;
                }
            }
            else {
                
                //now that we have the first id to lookup, we can start the process

                var self = this;
                var node = args.node;
                
                var doSync = function() {                    
                    //check if it exists in the already loaded children
                    var child = self.getChildNode(node, args.path[currPathIndex]);
                    if (child) {
                        if (args.path.length === (currPathIndex + 1)) {
                            //woot! synced the node
                            if (!args.forceReload) {
                                deferred.resolve(child);
                            }
                            else {
                                //even though we've found the node if forceReload is specified
                                //we want to go update this single node from the server
                                self.reloadNode(child).then(function(reloaded) {
                                    deferred.resolve(reloaded);
                                }, function() {
                                    deferred.reject();
                                });
                            }
                        }
                        else {
                            //now we need to recurse with the updated node/currPathIndex
                            currPathIndex++;
                            node = child;
                            //recurse
                            doSync();
                        }
                    }
                    else {
                        //the current node doesn't have it's children loaded, so go get them
                        self.loadNodeChildren({ node: node, section: node.section }).then(function () {
                            //ok, got the children, let's find it
                            var found = self.getChildNode(node, args.path[currPathIndex]);
                            if (found) {
                                if (args.path.length === (currPathIndex + 1)) {
                                    //woot! synced the node
                                    deferred.resolve(found);
                                }
                                else {
                                    //now we need to recurse with the updated node/currPathIndex
                                    currPathIndex++;
                                    node = found;
                                    //recurse
                                    doSync();
                                }
                            }
                            else {
                                //fail!
                                deferred.reject();
                            }
                        }, function () {
                            //fail!
                            deferred.reject();
                        });
                    }                   
                };

                //start
                doSync();
            }
            
            return deferred.promise;

        }
        
    };
}

angular.module('umbraco.services').factory('treeService', treeService);