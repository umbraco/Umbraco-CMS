/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTree
* @restrict E
**/
function umbTreeDirective($q, treeService, notificationsService) {

    return {
        restrict: 'E',
        replace: true,
        terminal: false,
        templateUrl: 'views/components/tree/umb-tree.html',
        scope: {
            section: '@',
            treealias: '@',
            hideoptions: '@',
            hideheader: '@',
            cachekey: '@',
            isdialog: '@',
            onlyInitialized: '@',
            //Custom query string arguments to pass in to the tree as a string, example: "startnodeid=123&something=value"
            customtreeparams: '@',
            enablecheckboxes: '@',
            enablelistviewsearch: '@',
            enablelistviewexpand: '@',
            api: '=?',
            onInit: '&?'
        },
        controller: function ($scope, $element) {

            var vm = this;

            var registeredCallbacks = {
                treeNodeExpanded: [],
                treeNodeSelect: [],
                treeLoaded: [],
                treeSynced: [],
                treeOptionsClick: [],
                treeNodeAltSelect: []
            };

            //this is the API exposed by this directive, for either hosting controllers or for other directives
            vm.callbacks = {
                treeNodeExpanded: function (f) {
                    registeredCallbacks.treeNodeExpanded.push(f);
                },
                treeNodeSelect: function (f) {
                    registeredCallbacks.treeNodeSelect.push(f);
                },
                treeLoaded: function (f) {
                    registeredCallbacks.treeLoaded.push(f);
                },
                treeSynced: function (f) {
                    registeredCallbacks.treeSynced.push(f);
                },
                treeOptionsClick: function (f) {
                    registeredCallbacks.treeOptionsClick.push(f);
                },
                treeNodeAltSelect: function (f) {
                    registeredCallbacks.treeNodeAltSelect.push(f);
                }
            };
            vm.emitEvent = emitEvent;
            vm.load = load;
            vm.reloadNode = reloadNode;
            vm.syncTree = syncTree;
            vm.loadChildren = loadChildren;
            vm.hasTree = hasTree;

            //wire up the exposed api object for hosting controllers
            if ($scope.api) {
                $scope.api.callbacks = vm.callbacks;
                $scope.api.load = vm.load;
                $scope.api.reloadNode = vm.reloadNode;
                $scope.api.syncTree = vm.syncTree;
                $scope.api.hasTree = vm.hasTree;
            }

            //flag to track the last loaded section when the tree 'un-loads'. We use this to determine if we should
            // re-load the tree again. For example, if we hover over 'content' the content tree is shown. Then we hover
            // outside of the tree and the tree 'un-loads'. When we re-hover over 'content', we don't want to re-load the
            // entire tree again since we already still have it in memory. Of course if the section is different we will
            // reload it. This saves a lot on processing if someone is navigating in and out of the same section many times
            // since it saves on data retreival and DOM processing.
            // TODO: This isn't used!?
            var lastSection = "";

            /** Helper function to emit tree events */
            function emitEvent(eventName, args) {
                if (registeredCallbacks[eventName] && Utilities.isArray(registeredCallbacks[eventName])) {
                    // call it
                    registeredCallbacks[eventName].forEach(c => c(args));
                }
            }


            /**
             * Re-loads the tree with the updated parameters
             * @param {any} args either a string representing the 'section' or an object containing: 'section', 'treeAlias', 'customTreeParams', 'cacheKey'
             */
            function load(args) {
                if (Utilities.isString(args)) {
                    $scope.section = args;
                }
                else if (args) {
                    if (args.section) {
                        $scope.section = args.section;
                    }
                    if (args.customTreeParams) {
                        $scope.customtreeparams = args.customTreeParams;
                    }
                    if (args.treeAlias) {
                        $scope.treealias = args.treeAlias;
                    }
                    if (args.cacheKey) {
                        $scope.cachekey = args.cacheKey;
                    }
                }

                return loadTree();
            }

            function reloadNode(node) {

                if (!node) {
                    node = $scope.currentNode;
                }

                if (node) {
                    return $scope.loadChildren(node, true);
                }

                return $q.reject();
            }

            /**
             * Used to do the tree syncing
             * @param {any} args
             * @returns a promise with an object containing 'node' and 'activate'
             */
            function syncTree(args) {
                if (!args) {
                    throw "args cannot be null";
                }
                if (!args.path) {
                    throw "args.path cannot be null";
                }

                if (Utilities.isString(args.path)) {
                    args.path = args.path.replace('"', '').split(',');
                }

                //Filter the path for root node ids (we don't want to pass in -1 or 'init')

                args.path = _.filter(args.path, function (item) { return (item !== "init" && item !== "-1"); });

                var treeNode = loadActiveTree(args.tree);

                return treeService.syncTree({
                    node: treeNode,
                    path: args.path,
                    forceReload: args.forceReload
                }).then(function (data) {

                    if (args.activate === undefined || args.activate === true) {
                        $scope.currentNode = data;
                    }

                    emitEvent("treeSynced", { node: data, activate: args.activate });

                    return $q.when({ node: data, activate: args.activate });
                }, function (data) {
                    return $q.reject(data);
                }, function (data) {
                    //on notification
                    if (data.type === "treeNodeExpanded") {
                        //raise the event
                        emitEvent("treeNodeExpanded", { tree: $scope.tree, node: data.node, children: data.children });
                    }
                });

            }

            /** This will check the section tree loaded and return all actual root nodes based on a tree type (non group nodes, non section groups) */
            function getTreeRootNodes() {
                var roots;
                if ($scope.tree.root.containsGroups) {
                    //all children in this case are group nodes, so we want the children of these children
                    roots = _.reduce(
                        //get the array of array of children
                        _.map($scope.tree.root.children, function (n) {
                            return n.children
                        }), function (m, p) {
                            //combine the arrays to one array
                            return m.concat(p)
                        });
                }
                else {
                    roots = [$scope.tree.root].concat($scope.tree.root.children);
                }

                return _.filter(roots, function (node) {
                    return node && node.metaData && node.metaData.treeAlias;
                });
            }

            //given a tree alias, this will search the current section tree for the specified tree alias and set the current active tree to it's root node
            function hasTree(treeAlias) {

                if (!$scope.tree) {
                    throw "Err in umbtree.directive.loadActiveTree, $scope.tree is null";
                }

                if (!treeAlias) {
                    return false;
                }

                var treeRoots = getTreeRootNodes();
                var foundTree = _.find(treeRoots, function (node) {
                    return node.metaData.treeAlias.toUpperCase() === treeAlias.toUpperCase();
                });

                return foundTree !== undefined;
            }

            //given a tree alias, this will search the current section tree for the specified tree alias and set the current active tree to it's root node
            function loadActiveTree(treeAlias) {

                if (!$scope.tree) {
                    throw "Err in umbtree.directive.loadActiveTree, $scope.tree is null";
                }

                //if its not specified, it should have been specified before
                if (!treeAlias) {
                    if (!$scope.activeTree) {
                        throw "Err in umbtree.directive.loadActiveTree, $scope.activeTree is null";
                    }
                    return $scope.activeTree;
                }

                var treeRoots = getTreeRootNodes();
                $scope.activeTree = _.find(treeRoots, function (node) {
                    return node.metaData.treeAlias.toUpperCase() === treeAlias.toUpperCase();
                });

                if (!$scope.activeTree) {
                    throw "Could not find the tree " + treeAlias;
                }

                emitEvent("activeTreeLoaded", { tree: $scope.activeTree });

                return $scope.activeTree;
            }

            /** Method to load in the tree data */
            function loadTree() {
                if ($scope.section) {

                    //default args
                    var args = { section: $scope.section, tree: $scope.treealias, cacheKey: $scope.cachekey, isDialog: $scope.isdialog ? $scope.isdialog : false };

                    //add the extra query string params if specified
                    if ($scope.customtreeparams) {
                        args["queryString"] = $scope.customtreeparams;
                    }

                    return treeService.getTree(args)
                        .then(function (data) {
                            //Only use the tree data, if we are still on the correct section
                            if(data.alias !== $scope.section){
                                return $q.reject();
                            }
                            
                            //set the data once we have it
                            $scope.tree = data;

                            //set the root as the current active tree
                            $scope.activeTree = $scope.tree.root;

                            emitEvent("treeLoaded", { tree: $scope.tree });
                            emitEvent("treeNodeExpanded", { tree: $scope.tree, node: $scope.tree.root, children: $scope.tree.root.children });
                         
                            return $q.when(data);
                        }, function (reason) {
                            notificationsService.error("Tree Error", reason);
                            return $q.reject(reason);
                        });
                }
                else {
                    return $q.reject();
                }
            }

            function loadChildren(node, forceReload) {
                //emit treeNodeExpanding event, if a callback object is set on the tree
                emitEvent("treeNodeExpanding", { tree: $scope.tree, node: node });

                //standardising
                if (!node.children) {
                    node.children = [];
                }

                if (forceReload || (node.hasChildren && node.children.length === 0)) {
                    //get the children from the tree service
                    return treeService.loadNodeChildren({ node: node, section: $scope.section, isDialog: $scope.isdialog })
                        .then(function (data) {
                            //emit expanded event
                            emitEvent("treeNodeExpanded", { tree: $scope.tree, node: node, children: data });

                            return $q.when(data);
                        });
                }
                else {
                    emitEvent("treeNodeExpanded", { tree: $scope.tree, node: node, children: node.children });
                    node.expanded = true;

                    return $q.when(node.children);
                }
            }

            /** Returns the css classses assigned to the node (div element) */
            $scope.getNodeCssClass = function (node) {
                if (!node) {
                    return '';
                }

                // TODO: This is called constantly because as a method in a template it's re-evaluated pretty much all the time
                // it would be better if we could cache the processing. The problem is that some of these things are dynamic.

                var css = [];
                if (node.cssClasses) {
                    node.cssClasses.forEach(c => css.push(c));
                }

                return css.join(" ");
            };

            $scope.selectEnabledNodeClass = node =>
                node && node.selected ? 'icon sprTree icon-check green temporary' : '-hidden';

            /* helper to force reloading children of a tree node */
            $scope.loadChildren = (node, forceReload) => loadChildren(node, forceReload);

            /**
              Method called when the options button next to the root node is called.
              The tree doesnt know about this, so it raises an event to tell the parent controller
              about it.
            */
            $scope.options = function (n, ev) {
                emitEvent("treeOptionsClick", { element: $element, node: n, event: ev });
            };

            /**
              Method called when an item is clicked in the tree, this passes the
              DOM element, the tree node object and the original click
              and emits it as a treeNodeSelect element if there is a callback object
              defined on the tree
            */
            $scope.select = function (n, ev) {
                
                if (n.metaData && n.metaData.noAccess === true) {
                    ev.preventDefault();
                    return;
                }

                //on tree select we need to remove the current node -
                // whoever handles this will need to make sure the correct node is selected
                //reset current node selection
                $scope.currentNode = null;

                emitEvent("treeNodeSelect", { element: $element, node: n, event: ev });
            };

            $scope.altSelect = function (n, ev) {
                emitEvent("treeNodeAltSelect", { element: $element, tree: $scope.tree, node: n, event: ev });
            };

            //call the onInit method, if the result is a promise then load the tree after that resolves (if it's not a promise this will just resolve automatically).
            //NOTE: The promise cannot be rejected, else the tree won't be loaded and we'll get exceptions if some API calls syncTree or similar.
            $q.when($scope.onInit(), function (args) {

                //the promise resolution can pass in parameters
                if (args) {
                    if (args.section) {
                        $scope.section = args.section;
                    }
                    if (args.cacheKey) {
                        $scope.cachekey = args.cacheKey;
                    }
                    if (args.customTreeParams) {
                        $scope.customtreeparams = args.customTreeParams;
                    }
                }

                //load the tree
                loadTree().then(function () {
                    //because angular doesn't return a promise for the resolve method, we need to resort to some hackery, else
                    //like normal JS promises we could do resolve(...).then()
                    if (args && args.onLoaded && Utilities.isFunction(args.onLoaded)) {
                        args.onLoaded();
                    }
                });
            });
        }
    };
}

angular.module("umbraco.directives").directive('umbTree', umbTreeDirective);
