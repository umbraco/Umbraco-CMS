/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTree
* @restrict E
**/
function umbTreeDirective($compile, $log, $q, $rootScope, treeService, notificationsService, $timeout, userService) {

    return {
        restrict: 'E',
        replace: true,
        terminal: false,

        scope: {
            section: '@',
            treealias: '@',
            hideoptions: '@',
            hideheader: '@',
            cachekey: '@',
            isdialog: '@',
            //Custom query string arguments to pass in to the tree as a string, example: "startnodeid=123&something=value"
            customtreeparams: '@',
            eventhandler: '=',
            enablecheckboxes: '@',
            enablelistviewsearch: '@'
        },

        compile: function(element, attrs) {
            //config
            //var showheader = (attrs.showheader !== 'false');
            var hideoptions = (attrs.hideoptions === 'true') ? "hide-options" : "";
            var template = '<ul class="umb-tree ' + hideoptions + '"><li class="root">';
            template += '<div ng-hide="hideheader" on-right-click="altSelect(tree.root, $event)">' +
                '<h5>' +
                '<i ng-if="enablecheckboxes == \'true\'" ng-class="selectEnabledNodeClass(tree.root)"></i>' +
                '<a href="#/{{section}}" ng-click="select(tree.root, $event)"  class="root-link">{{tree.name}}</a></h5>' +
                '<a class="umb-options" ng-hide="tree.root.isContainer || !tree.root.menuUrl" ng-click="options(tree.root, $event)" ng-swipe-right="options(tree.root, $event)"><i></i><i></i><i></i></a>' +
                '</div>';
            template += '<ul>' +
                '<umb-tree-item ng-repeat="child in tree.root.children" eventhandler="eventhandler" node="child" current-node="currentNode" tree="this" section="{{section}}" ng-animate="animation()"></umb-tree-item>' +
                '</ul>' +
                '</li>' +
                '</ul>';

            element.replaceWith(template);

            return function(scope, elem, attr, controller) {

                //flag to track the last loaded section when the tree 'un-loads'. We use this to determine if we should
                // re-load the tree again. For example, if we hover over 'content' the content tree is shown. Then we hover
                // outside of the tree and the tree 'un-loads'. When we re-hover over 'content', we don't want to re-load the 
                // entire tree again since we already still have it in memory. Of course if the section is different we will
                // reload it. This saves a lot on processing if someone is navigating in and out of the same section many times
                // since it saves on data retreival and DOM processing.
                var lastSection = "";
                
                //setup a default internal handler
                if (!scope.eventhandler) {
                    scope.eventhandler = $({});
                }

                //flag to enable/disable delete animations
                var deleteAnimations = false;


                /** Helper function to emit tree events */
                function emitEvent(eventName, args) {
                    if (scope.eventhandler) {
                        $(scope.eventhandler).trigger(eventName, args);
                    }
                }
                
                /** This will deleteAnimations to true after the current digest */
                function enableDeleteAnimations() {
                    //do timeout so that it re-enables them after this digest
                    $timeout(function () {
                        //enable delete animations
                        deleteAnimations = true;
                    }, 0, false);
                }


                /*this is the only external interface a tree has */
                function setupExternalEvents() {
                    if (scope.eventhandler) {

                        scope.eventhandler.clearCache = function(section) {
                            treeService.clearCache({ section: section });
                        };

                        scope.eventhandler.load = function(section) {
                            scope.section = section;
                            loadTree();
                        };

                        scope.eventhandler.reloadNode = function(node) {

                            if (!node) {
                                node = scope.currentNode;
                            }

                            if (node) {
                                scope.loadChildren(node, true);
                            }
                        };
                        
                        /** 
                            Used to do the tree syncing. If the args.tree is not specified we are assuming it has been 
                            specified previously using the _setActiveTreeType
                        */
                        scope.eventhandler.syncTree = function(args) {
                            if (!args) {
                                throw "args cannot be null";
                            }
                            if (!args.path) {
                                throw "args.path cannot be null";
                            }
                            
                            var deferred = $q.defer();

                            //this is super complex but seems to be working in other places, here we're listening for our
                            // own events, once the tree is sycned we'll resolve our promise.
                            scope.eventhandler.one("treeSynced", function (e, syncArgs) {
                                deferred.resolve(syncArgs);
                            });

                            //this should normally be set unless it is being called from legacy 
                            // code, so set the active tree type before proceeding.
                            if (args.tree) {
                                loadActiveTree(args.tree);
                            }

                            if (angular.isString(args.path)) {
                                args.path = args.path.replace('"', '').split(',');
                            }

                            //reset current node selection
                            //scope.currentNode = null;

                            //Filter the path for root node ids (we don't want to pass in -1 or 'init')

                            args.path = _.filter(args.path, function (item) { return (item !== "init" && item !== "-1"); });

                            //Once those are filtered we need to check if the current user has a special start node id, 
                            // if they do, then we're going to trim the start of the array for anything found from that start node
                            // and previous so that the tree syncs properly. The tree syncs from the top down and if there are parts
                            // of the tree's path in there that don't actually exist in the dom/model then syncing will not work.

                            userService.getCurrentUser().then(function(userData) {

                                var startNodes = [userData.startContentId, userData.startMediaId];
                                _.each(startNodes, function (i) {
                                    var found = _.find(args.path, function (p) {
                                        return String(p) === String(i);
                                    });
                                    if (found) {
                                        args.path = args.path.splice(_.indexOf(args.path, found));
                                    }
                                });


                                loadPath(args.path, args.forceReload, args.activate);

                            });

                            

                            return deferred.promise;
                        };

                        /** 
                            Internal method that should ONLY be used by the legacy API wrapper, the legacy API used to 
                            have to set an active tree and then sync, the new API does this in one method by using syncTree.
                            loadChildren is optional but if it is set, it will set the current active tree and load the root
                            node's children - this is synonymous with the legacy refreshTree method - again should not be used
                            and should only be used for the legacy code to work.
                        */
                        scope.eventhandler._setActiveTreeType = function(treeAlias, loadChildren) {
                            loadActiveTree(treeAlias, loadChildren);
                        };
                    }
                }


                //helper to load a specific path on the active tree as soon as its ready
                function loadPath(path, forceReload, activate) {

                    if (scope.activeTree) {
                        syncTree(scope.activeTree, path, forceReload, activate);
                    }
                    else {
                        scope.eventhandler.one("activeTreeLoaded", function (e, args) {
                            syncTree(args.tree, path, forceReload, activate);
                        });
                    }
                }

                
                //given a tree alias, this will search the current section tree for the specified tree alias and
                //set that to the activeTree
                //NOTE: loadChildren is ONLY used for legacy purposes, do not use this when syncing the tree as it will cause problems
                // since there will be double request and event handling operations.
                function loadActiveTree(treeAlias, loadChildren) {
                    scope.activeTree = undefined;

                    function doLoad(tree) {
                        var childrenAndSelf = [tree].concat(tree.children);
                        scope.activeTree = _.find(childrenAndSelf, function (node) {
                            if(node && node.metaData){
                                return node.metaData.treeAlias === treeAlias;
                            }
                            return false;
                        });
                        
                        if (!scope.activeTree) {
                            throw "Could not find the tree " + treeAlias + ", activeTree has not been set";
                        }

                        //This is only used for the legacy tree method refreshTree!
                        if (loadChildren) {
                            scope.activeTree.expanded = true;
                            scope.loadChildren(scope.activeTree, false).then(function() {
                                emitEvent("activeTreeLoaded", { tree: scope.activeTree });
                            });
                        }
                        else {
                            emitEvent("activeTreeLoaded", { tree: scope.activeTree });
                        }
                    }

                    if (scope.tree) {
                        doLoad(scope.tree.root);
                    }
                    else {
                        scope.eventhandler.one("treeLoaded", function(e, args) {
                            doLoad(args.tree.root);
                        });
                    }
                }


                /** Method to load in the tree data */

                function loadTree() {
                    if (!scope.loading && scope.section) {
                        scope.loading = true;

                        //anytime we want to load the tree we need to disable the delete animations
                        deleteAnimations = false;

                        //default args
                        var args = { section: scope.section, tree: scope.treealias, cacheKey: scope.cachekey, isDialog: scope.isdialog ? scope.isdialog : false };

                        //add the extra query string params if specified
                        if (scope.customtreeparams) {
                            args["queryString"] = scope.customtreeparams;
                        }

                        treeService.getTree(args)
                            .then(function(data) {
                                //set the data once we have it
                                scope.tree = data;
                                
                                enableDeleteAnimations();

                                scope.loading = false;

                                //set the root as the current active tree
                                scope.activeTree = scope.tree.root;
                                emitEvent("treeLoaded", { tree: scope.tree });
                                emitEvent("treeNodeExpanded", { tree: scope.tree, node: scope.tree.root, children: scope.tree.root.children });
                           
                            }, function(reason) {
                                scope.loading = false;
                                notificationsService.error("Tree Error", reason);
                            });
                    }
                }

                /** syncs the tree, the treeNode can be ANY tree node in the tree that requires syncing */
                function syncTree(treeNode, path, forceReload, activate) {

                    deleteAnimations = false;

                    treeService.syncTree({
                        node: treeNode,
                        path: path,
                        forceReload: forceReload
                    }).then(function (data) {

                        if (activate === undefined || activate === true) {
                            scope.currentNode = data;
                        }
                        
                        emitEvent("treeSynced", { node: data, activate: activate });
                        
                        enableDeleteAnimations();
                    });

                }

                scope.selectEnabledNodeClass = function (node) {
                    return node ?
                        node.selected ?
                        'icon umb-tree-icon sprTree icon-check blue temporary' :
                        '' :
                        '';
                };

                /** method to set the current animation for the node. 
                 *  This changes dynamically based on if we are changing sections or just loading normal tree data. 
                 *  When changing sections we don't want all of the tree-ndoes to do their 'leave' animations.
                 */
                scope.animation = function() {
                    if (deleteAnimations && scope.tree && scope.tree.root && scope.tree.root.expanded) {
                        return { leave: 'tree-node-delete-leave' };
                    }
                    else {
                        return {};
                    }
                };

                /* helper to force reloading children of a tree node */
                scope.loadChildren = function(node, forceReload) {
                    var deferred = $q.defer();

                    //emit treeNodeExpanding event, if a callback object is set on the tree
                    emitEvent("treeNodeExpanding", { tree: scope.tree, node: node });

                    //standardising                
                    if (!node.children) {
                        node.children = [];
                    }

                    if (forceReload || (node.hasChildren && node.children.length === 0)) {
                        //get the children from the tree service
                        treeService.loadNodeChildren({ node: node, section: scope.section })
                            .then(function(data) {
                                //emit expanded event
                                emitEvent("treeNodeExpanded", { tree: scope.tree, node: node, children: data });
                                
                                enableDeleteAnimations();

                                deferred.resolve(data);
                            });
                    }
                    else {
                        emitEvent("treeNodeExpanded", { tree: scope.tree, node: node, children: node.children });
                        node.expanded = true;
                        
                        enableDeleteAnimations();

                        deferred.resolve(node.children);
                    }

                    return deferred.promise;
                };

                /**
                  Method called when the options button next to the root node is called.
                  The tree doesnt know about this, so it raises an event to tell the parent controller
                  about it.
                */
                scope.options = function(n, ev) {
                    emitEvent("treeOptionsClick", { element: elem, node: n, event: ev });
                };

                /**
                  Method called when an item is clicked in the tree, this passes the 
                  DOM element, the tree node object and the original click
                  and emits it as a treeNodeSelect element if there is a callback object
                  defined on the tree
                */
                scope.select = function (n, ev) {
                    //on tree select we need to remove the current node - 
                    // whoever handles this will need to make sure the correct node is selected
                    //reset current node selection
                    scope.currentNode = null;

                    emitEvent("treeNodeSelect", { element: elem, node: n, event: ev });
                };

                scope.altSelect = function(n, ev) {
                    emitEvent("treeNodeAltSelect", { element: elem, tree: scope.tree, node: n, event: ev });
                };
                
                //watch for section changes
                scope.$watch("section", function(newVal, oldVal) {

                    if (!scope.tree) {
                        loadTree();
                    }

                    if (!newVal) {
                        //store the last section loaded
                        lastSection = oldVal;
                    }
                    else if (newVal !== oldVal && newVal !== lastSection) {
                        //only reload the tree data and Dom if the newval is different from the old one
                        // and if the last section loaded is different from the requested one.
                        loadTree();

                        //store the new section to be loaded as the last section
                        //clear any active trees to reset lookups
                        lastSection = newVal;
                    }
                });
                
                setupExternalEvents();
                loadTree();
            };
        }
    };
}

angular.module("umbraco.directives").directive('umbTree', umbTreeDirective);
