/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTree
* @restrict E
**/
angular.module("umbraco.directives")
  .directive('umbTree', function ($compile, $log, $q, $rootScope, navigationService, treeService, notificationsService, $timeout) {
    
    return {
      restrict: 'E',
      replace: true,
      terminal: false,

      scope: {
        section: '@',
        treealias: '@',
        showoptions: '@',
        showheader: '@',
        cachekey: '@',
        isdialog: '@',
        eventhandler: '='
      },

      compile: function (element, attrs) {
         //config
         var hideheader = (attrs.showheader === 'false') ? true : false;
         var hideoptions = (attrs.showoptions === 'false') ? "hide-options" : "";
         
         var template = '<ul class="umb-tree ' + hideoptions + '">' + 
         '<li class="root">';

         if(!hideheader){ 
           template +='<div>' + 
           '<h5><a href="#/{{section}}" ng-click="select(this, tree.root, $event)" on-right-click="altSelect(this, tree.root, $event)"  class="root-link">{{tree.name}}</a></h5>' +
               '<a href class="umb-options" ng-hide="tree.root.isContainer || !tree.root.menuUrl" ng-click="options(this, tree.root, $event)" ng-swipe-right="options(this, tree.root, $event)"><i></i><i></i><i></i></a>' +
           '</div>';
         }
         template += '<ul>' +
                  '<umb-tree-item ng-repeat="child in tree.root.children" eventhandler="eventhandler" path="{{path}}" activetree="{{activetree}}" node="child" current-node="currentNode" tree="child" section="{{section}}" ng-animate="animation()"></umb-tree-item>' +
                  '</ul>' +
                '</li>' +
               '</ul>';

        element.replaceWith(template);

        return function (scope, elem, attr, controller) {

            //flag to track the last loaded section when the tree 'un-loads'. We use this to determine if we should
            // re-load the tree again. For example, if we hover over 'content' the content tree is shown. Then we hover
            // outside of the tree and the tree 'un-loads'. When we re-hover over 'content', we don't want to re-load the 
            // entire tree again since we already still have it in memory. Of course if the section is different we will
            // reload it. This saves a lot on processing if someone is navigating in and out of the same section many times
            // since it saves on data retreival and DOM processing.
            var lastSection = "";
            
            //keeps track of the currently active tree being called by editors syncing
            var activeTree;

            //setup a default internal handler
            if(!scope.eventhandler){
              scope.eventhandler = $({});
            }
            
            //flag to enable/disable delete animations
            var enableDeleteAnimations = false;


            /** Helper function to emit tree events */
            function emitEvent(eventName, args) {
              if (scope.eventhandler) {
                $(scope.eventhandler).trigger(eventName, args);
              }
            }


            /*this is the only external interface a tree has */
            function setupExternalEvents() {
              if (scope.eventhandler) {
                
                scope.eventhandler.clearCache = function(section){
                    treeService.clearCache({ section: section });
                };

                scope.eventhandler.load = function(section){
                  scope.section = section;
                  loadTree();
                };

                scope.eventhandler.reloadNode = function(node){

                    if(!node){
                      node = scope.currentNode;
                    }
                      
                    if(node){
                      scope.loadChildren(node, true); 
                    }
                };

                scope.eventhandler.syncPath = function(path, forceReload){

                  if(angular.isString(path)){
                    path = path.replace('"', '').split(',');
                  }

                  //reset current node selection
                  scope.currentNode = undefined;
                  navigationService.ui.currentNode = undefined;

                  //filter the path for root node ids
                  path = _.filter(path, function(item){ return (item !== "init" && item !== "-1"); });
                  loadPath(path, forceReload);
                };
                
                scope.eventhandler.setActiveTreeType = function(treeAlias){
                    loadActiveTree(treeAlias);
                };
              }
            }




            //helper to load a specific path on the active tree as soon as its ready
            function loadPath(path, forceReload){
              function _load(tree, path, forceReload){
                  syncTree(tree, path, forceReload);
              }

              if(scope.activeTree){
                _load(scope.activeTree, path, forceReload);
              }else{
                scope.eventhandler.one("activeTreeLoaded", function(e, args){
                  _load(args.tree, path, forceReload);
                });
              }
            }

            //expands the first child with a tree alias as soon as the tree has loaded
            function loadActiveTree(treeAlias){
                scope.activeTree = undefined;

                function _load(tree, alias){
                  scope.activeTree = _.find(tree.children, function(node){ return node.metaData.treeAlias === treeAlias; });
                  scope.activeTree.expanded = true;
                  
                  scope.loadChildren(scope.activeTree, false).then(function(){
                    emitEvent("activeTreeLoaded", {tree: scope.activeTree});
                  });
                }

                if(scope.tree){
                  _load(scope.tree.root, treeAlias);
                }else{
                  scope.eventhandler.one("treeLoaded", function(e, args){
                    _load(args.tree, treeAlias);
                  });
                }
            }


            /** Method to load in the tree data */
            function loadTree() {                
                if (!scope.loading && scope.section) {
                    scope.loading = true;

                    //anytime we want to load the tree we need to disable the delete animations
                    enableDeleteAnimations = false;

                    //use $q.when because a promise OR raw data might be returned.
                    treeService.getTree({ section: scope.section, tree: scope.treealias, cacheKey: scope.cachekey, isDialog: scope.isdialog ? scope.isdialog : false })
                        .then(function (data) {
                            //set the data once we have it
                            scope.tree = data;
                            
                            

                            //do timeout so that it re-enables them after this digest
                            $timeout(function() {
                                //enable delete animations
                                enableDeleteAnimations = true;
                            },0,false);

                            scope.loading = false;

                            //set the root as the current active tree
                            scope.activeTree = scope.tree.root;
                            emitEvent("treeLoaded", {tree: scope.tree.root});
                              
                        }, function (reason) {
                            scope.loading = false;
                            notificationsService.error("Tree Error", reason);
                        });
                }
            }

            function syncTree(node, path, forceReload) {
              if(!node || !path || path.length === 0){
                return;
              }

              //we are directly above the changed node
              var onParent = (path.length === 1);
              var needsReload = true;

              node.expanded = true;

              //if we are not directly above, we will just try to locate
              //the node and continue down the path
              if(!onParent){
                //if we can find the next node in the path 
                var child = treeService.getChildNode(node, path[0]);
                if(child){
                  needsReload = false;
                  path.splice(0,1);
                  syncTree(child, path, forceReload);
                }
              }
              
              //if a reload is needed, all children will be loaded from server
              if(needsReload){
                  scope.loadChildren(node, forceReload)
                    .then(function(children){
                        var child = treeService.getChildNode(node, path[0]);
                        
                        if(!onParent){
                          path.splice(0,1);
                          syncTree(child, path, forceReload);
                        }
                        else {
                            navigationService.ui.currentNode = child;
                              scope.currentNode = child;
                        }
                    });
              }
            }

           

            /** method to set the current animation for the node. 
             *  This changes dynamically based on if we are changing sections or just loading normal tree data. 
             *  When changing sections we don't want all of the tree-ndoes to do their 'leave' animations.
             */
            scope.animation = function () {
                if (enableDeleteAnimations && scope.tree && scope.tree.root && scope.tree.root.expanded) {
                    return { leave: 'tree-node-delete-leave' };
                }
                else {
                    return {};
                }
            };

            /* helper to force reloading children of a tree node */
            scope.loadChildren = function(node, forceReload){           
                var deferred = $q.defer();

                //emit treeNodeExpanding event, if a callback object is set on the tree
                emitEvent("treeNodeExpanding", {tree: scope.tree, node: node });
  
                //standardising                
                if(!node.children){
                  node.children = [];
                }

                if (forceReload || (node.hasChildren && node.children.length === 0)) {
                    //get the children from the tree service
                    treeService.loadNodeChildren({ node: node, section: scope.section })
                        .then(function(data) {
                            //emit expanded event
                            emitEvent("treeNodeExpanded", { tree: scope.tree, node: node, children: data });
                            enableDeleteAnimations = true;

                            deferred.resolve(data);
                        });
                }
                else {
                    emitEvent("treeNodeExpanded", {tree: scope.tree, node: node, children: node.children });
                    node.expanded = true;
                    enableDeleteAnimations = true;

                    deferred.resolve(node.children);
                }

                return deferred.promise;
            };

            /**
              Method called when the options button next to the root node is called.
              The tree doesnt know about this, so it raises an event to tell the parent controller
              about it.
            */
            scope.options = function (e, n, ev) {
                emitEvent("treeOptionsClick", { element: e, node: n, event: ev });
            };
              
            /**
              Method called when an item is clicked in the tree, this passes the 
              DOM element, the tree node object and the original click
              and emits it as a treeNodeSelect element if there is a callback object
              defined on the tree
            */
            scope.select = function(e,n,ev){
                emitEvent("treeNodeSelect", { element: e, node: n, event: ev });
            };
            
            scope.altSelect = function(e,n,ev){
                emitEvent("treeNodeAltSelect", { element: e, tree: scope.tree, node: n, event: ev });
            };
            

            //watch for section changes
            scope.$watch("section", function (newVal, oldVal) {
                  
                  if(!scope.tree){
                    loadTree();  
                  }

                  if (!newVal) {
                      //store the last section loaded
                      lastSection = oldVal;
                  }else if (newVal !== oldVal && newVal !== lastSection) {
                      //only reload the tree data and Dom if the newval is different from the old one
                      // and if the last section loaded is different from the requested one.
                      loadTree();
                      
                      //store the new section to be loaded as the last section
                      //clear any active trees to reset lookups
                      lastSection = newVal;
                      activeTree = undefined;
                  } 
            });

            //When the user logs in
            scope.$on("authenticated", function (evt, data) {
                //populate the tree if the user has changed
                if (data.lastUserId !== data.user.id) {
                    treeService.clearCache();
                    scope.tree = null;

                    setupExternalEvents();
                    loadTree();
                }
            });
            
         };
       }
      };
    });