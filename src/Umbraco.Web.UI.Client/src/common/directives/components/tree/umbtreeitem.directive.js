/**
 * @ngdoc directive
 * @name umbraco.directives.directive:umbTreeItem
 * @element li
 * @function
 *
 * @description
 * Renders a list item, representing a single node in the tree.
 * Includes element to toggle children, and a menu toggling button
 *
 * **note:** This directive is only used internally in the umbTree directive
 *
 * @example
   <example module="umbraco">
    <file name="index.html">
         <umb-tree-item ng-repeat="child in tree.children" node="child" callback="callback" section="content"></umb-tree-item>
    </file>
   </example>
 */
angular.module("umbraco.directives")
.directive('umbTreeItem', function ($compile, $http, $templateCache, $interpolate, $log, $location, $rootScope, $window, treeService, $timeout, localizationService) {
    return {
        restrict: 'E',
        replace: true,
        require: '^umbTree',
        templateUrl: 'views/components/tree/umb-tree-item.html',
        scope: {
            section: '@',
            currentNode: '=',
            enablelistviewexpand: '@',
            node: '=',
            tree: '=' //TODO: Not sure we need this since we are 'require' on the umbTree
        },
        
        link: function (scope, element, attrs, umbTreeCtrl) {

            localizationService.localize("general_search").then(function (value) {
                scope.searchAltText = value;
            });

            //flag to enable/disable delete animations, default for an item is true
            scope.deleteAnimations = true;

            // updates the node's DOM/styles
            function setupNodeDom(node, tree) {
                
                //get the first div element
                element.children(":first")
                    //set the padding
                    .css("padding-left", (node.level * 20) + "px");

                // add a unique data element to each tree item so it is easy to navigate with code
                if(!node.metaData.treeAlias) {
                    node.dataElement = node.name;
                } else {
                    node.dataElement = node.metaData.treeAlias;
                }

            }

            //This will deleteAnimations to true after the current digest
            function enableDeleteAnimations() {
                //do timeout so that it re-enables them after this digest
                $timeout(function () {
                    //enable delete animations
                    scope.deleteAnimations = true;
                }, 0, false);
            }

            /** Returns the css classses assigned to the node (div element) */
            scope.getNodeCssClass = function (node) {
                if (!node) {
                    return '';
                }

                //TODO: This is called constantly because as a method in a template it's re-evaluated pretty much all the time
                // it would be better if we could cache the processing. The problem is that some of these things are dynamic.

                var css = [];                
                if (node.cssClasses) {
                    _.each(node.cssClasses, function(c) {
                        css.push(c);
                    });
                }
                if (node.selected) {
                    css.push("umb-tree-node-checked");
                }
                
                return css.join(" ");
            };

            //add a method to the node which we can use to call to update the node data if we need to ,
            // this is done by sync tree, we don't want to add a $watch for each node as that would be crazy insane slow
            // so we have to do this
            scope.node.updateNodeData = function (newNode) {
                _.extend(scope.node, newNode);
                //now update the styles
                setupNodeDom(scope.node, scope.tree);
            };

            /**
              Method called when the options button next to a node is called
              In the main tree this opens the menu, but internally the tree doesnt
              know about this, so it simply raises an event to tell the parent controller
              about it.
            */
            scope.options = function (n, ev) {
                umbTreeCtrl.emitEvent("treeOptionsClick", { element: element, tree: scope.tree, node: n, event: ev });
            };

            /**
              Method called when an item is clicked in the tree, this passes the 
              DOM element, the tree node object and the original click
              and emits it as a treeNodeSelect element if there is a callback object
              defined on the tree
            */
            scope.select = function (n, ev) {
                if (ev.ctrlKey ||
                    ev.shiftKey ||
                    ev.metaKey || // apple
                    (ev.button && ev.button === 1) // middle click, >IE9 + everyone else
                ) {
                    return;
                }

                if (n.metaData && n.metaData.noAccess === true) {
                    ev.preventDefault();
                    return;
                }

                umbTreeCtrl.emitEvent("treeNodeSelect", { element: element, tree: scope.tree, node: n, event: ev });
                ev.preventDefault();
            };

            /**
              Method called when an item is right-clicked in the tree, this passes the 
              DOM element, the tree node object and the original click
              and emits it as a treeNodeSelect element if there is a callback object
              defined on the tree
            */
            scope.altSelect = function (n, ev) {
                umbTreeCtrl.emitEvent("treeNodeAltSelect", { element: element, tree: scope.tree, node: n, event: ev });
            };

            /** method to set the current animation for the node. 
            *  This changes dynamically based on if we are changing sections or just loading normal tree data. 
            *  When changing sections we don't want all of the tree-ndoes to do their 'leave' animations.
            */
            scope.animation = function () {
                if (scope.node.showHideAnimation) {
                    return scope.node.showHideAnimation;
                }
                if (scope.deleteAnimations && scope.node.expanded) {
                    return { leave: 'tree-node-delete-leave' };
                }
                else {
                    return {};
                }
            };

            /**
              Method called when a node in the tree is expanded, when clicking the arrow
              takes the arrow DOM element and node data as parameters
              emits treeNodeCollapsing event if already expanded and treeNodeExpanding if collapsed
            */
            scope.load = function (node) {
                if (node.expanded && !node.metaData.isContainer) {
                    scope.deleteAnimations = false;
                    umbTreeCtrl.emitEvent("treeNodeCollapsing", { tree: scope.tree, node: node, element: element });
                    node.expanded = false;
                }
                else {
                    scope.loadChildren(node, false);
                }
            };

            /* helper to force reloading children of a tree node */
            scope.loadChildren = function (node, forceReload) {
                //emit treeNodeExpanding event, if a callback object is set on the tree
                umbTreeCtrl.emitEvent("treeNodeExpanding", { tree: scope.tree, node: node });

                if (node.hasChildren && (forceReload || !node.children || (angular.isArray(node.children) && node.children.length === 0))) {
                    //get the children from the tree service
                    treeService.loadNodeChildren({ node: node, section: scope.section })
                        .then(function (data) {
                            //emit expanded event
                            umbTreeCtrl.emitEvent("treeNodeExpanded", { tree: scope.tree, node: node, children: data });
                            enableDeleteAnimations();
                        });
                }
                else {
                    umbTreeCtrl.emitEvent("treeNodeExpanded", { tree: scope.tree, node: node, children: node.children });
                    node.expanded = true;
                    enableDeleteAnimations();
                }
            };            

            //if the current path contains the node id, we will auto-expand the tree item children

            setupNodeDom(scope.node, scope.tree);

            // load the children if the current user don't have access to the node
            // it is used to auto expand the tree to the start nodes the user has access to
            if(scope.node.hasChildren && scope.node.metaData.noAccess) {
                scope.loadChildren(scope.node);
            }
          
        }
    };
});
