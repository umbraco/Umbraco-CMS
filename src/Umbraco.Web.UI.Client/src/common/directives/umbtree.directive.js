/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTree
* @restrict E
**/
angular.module("umbraco.directives")
  .directive('umbTree', function ($compile, $log, $q, $rootScope, treeService, notificationsService, $timeout) {
    
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

        return function (scope, element, attrs, controller) {

            //flag to track the last loaded section when the tree 'un-loads'. We use this to determine if we should
            // re-load the tree again. For example, if we hover over 'content' the content tree is shown. Then we hover
            // outside of the tree and the tree 'un-loads'. When we re-hover over 'content', we don't want to re-load the 
            // entire tree again since we already still have it in memory. Of course if the section is different we will
            // reload it. This saves a lot on processing if someone is navigating in and out of the same section many times
            // since it saves on data retreival and DOM processing.
            var lastSection = "";
            
            //keeps track of the currently active tree being called by editors syncing
            var activeTree;

            //flag to enable/disable delete animations
            var enableDeleteAnimations = false;

            /** Helper function to emit tree events */
            function emitEvent(eventName, args) {
              if (scope.eventhandler) {
                $(scope.eventhandler).trigger(eventName, args);
              }
            }

            function setupExternalEvents() {
              if (scope.eventhandler) {
                
                scope.eventhandler.clearCache = function(treeAlias){
                  treeService.clearCache(treeAlias);
                };

                scope.eventhandler.syncPath = function(path, forceReload){
                  if(!angular.isArray(path)){
                    path = path.split(',');
                  }
                  //reset current node selection
                  scope.currentNode = undefined;

                  //filter the path for root node ids
                  path = _.filter(path, function(item){ return (item !== "init" && item !== "-1"); });
                   
                  //if we have a active tree, we sync based on that.
                  var root = activeTree ? activeTree : scope.tree.root;

                   //tell the tree to sync the children below the root
                   syncTree(root, path, forceReload);
                };
                
                scope.eventhandler.setActiveTreeType = function(treeAlias){
                    activeTree = _.find(scope.tree.root.children, function(node){ return node.metaData.treeAlias === treeAlias; });
                };
              }
            }

            /** Method to load in the tree data */
            function loadTree() {                
                if (!scope.loading && scope.section) {
                    scope.loading = true;

                    //anytime we want to load the tree we need to disable the delete animations
                    enableDeleteAnimations = false;

                    //use $q.when because a promise OR raw data might be returned.
                    $q.when(treeService.getTree({ section: scope.section, tree: scope.treealias, cachekey: scope.cachekey }))
                        .then(function (data) {
                            //set the data once we have it
                            scope.tree = data;

                            //do timeout so that it re-enables them after this digest
                            $timeout(function() {
                                
                                //enable delete animations
                                enableDeleteAnimations = true;
                            });

                            scope.loading = false;
                        }, function (reason) {
                            scope.loading = false;
                            notificationsService.error("Tree Error", reason);
                        });
                }
            }

            function syncTree(node, array, forceReload) {
              if(!node || !array || array.length === 0){
                return;
              }

              scope.loadChildren(node, forceReload)
                .then(function(children){
                    var next = _.where(children, {id: array[0]});
                    if(next && next.length > 0){
                      
                      if(array.length > 0){
                        array.splice(0,1);
                      }else{

                      }
                      
                      if(array.length === 0){
                          scope.currentNode = next[0];
                      }

                      syncTree(next[0], array, forceReload);
                    }
                });
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
                
                if (node.hasChildren && (forceReload || !node.children || (angular.isArray(node.children) && node.children.length === 0))) {
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
            
            //When a section is double clicked
            scope.$on("tree.clearCache", function (evt, data) {
                treeService.clearCache(data.tree);
                scope.tree = null;
            });  
            
         };
       }
      };
    });