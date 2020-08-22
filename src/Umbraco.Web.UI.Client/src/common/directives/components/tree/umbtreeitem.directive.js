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
    .directive('umbTreeItem', function(treeService, $timeout, localizationService, eventsService, appState, navigationService) {
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
            tree: '=',
            isDialog: '='
        },
        
        link: function (scope, element, attrs, umbTreeCtrl) {
            localizationService.localizeMany(["general_search", "visuallyHiddenTexts_openContextMenu"]).then(function (value) {
                scope.searchAltText = value[0];
                scope.optionsText = value[1];
            });
            
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

            /** Returns the css classses assigned to the node (div element) */
            scope.getNodeCssClass = function (node) {
                if (!node) {
                    return '';
                }
                
                // TODO: This is called constantly because as a method in a template it's re-evaluated pretty much all the time
                // it would be better if we could cache the processing. The problem is that some of these things are dynamic.

                //is this the current action node (this is not the same as the current selected node!)
                var actionNode = appState.getMenuState("currentNode");
                
                var css = [];                
                if (node.cssClasses) {
                    node.cssClasses.forEach(c => css.push(c));
                }
                if (node.selected) {
                    css.push("umb-tree-node-checked");
                }
                if (node == scope.currentNode) {
                    css.push("current");
                    if (actionNode && actionNode.id !== node.id) {
                        css.push("current-not-active");// when its the current node, but its not the active(current node for the given action)
                    }
                }
                if (node.hasChildren) {
                    css.push("has-children");
                }
                if (node.deleteAnimations) {
                    css.push("umb-tree-item--deleted");
                }

                // checking the nodeType to ensure that this node and actionNode is from the same treeAlias
                if (actionNode && actionNode.nodeType === node.nodeType) {

                    if (actionNode.id === node.id && String(node.id) !== "-1") {
                        css.push("active");
                    }
                    
                    // special handling of root nodes with id -1 
                    // as there can be many nodes with id -1 in a tree we need to check the treeAlias instead
                    if (String(node.id) === "-1" && actionNode.metaData.treeAlias === node.metaData.treeAlias) {
                        css.push("active");
                    }
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
            scope.altSelect = function(n, ev) {
                if(ev.altKey) return false;
                umbTreeCtrl.emitEvent("treeNodeAltSelect", { element: element, tree: scope.tree, node: n, event: ev });
            };
            
            /**
              Method called when a node in the tree is expanded, when clicking the arrow
              takes the arrow DOM element and node data as parameters
              emits treeNodeCollapsing event if already expanded and treeNodeExpanding if collapsed
            */
            scope.load = function (node) {
                if (node.expanded && !node.metaData.isContainer) {
                    umbTreeCtrl.emitEvent("treeNodeCollapsing", { tree: scope.tree, node: node, element: element });
                    node.expanded = false;
                }
                else {
                    scope.loadChildren(node, false);
                }
            };

            /* helper to force reloading children of a tree node */
            scope.loadChildren = function(node, forceReload) {
                return umbTreeCtrl.loadChildren(node, forceReload);
            };

            //if the current path contains the node id, we will auto-expand the tree item children
            setupNodeDom(scope.node, scope.tree);

            // load the children if the current user don't have access to the node
            // it is used to auto expand the tree to the start nodes the user has access to
            if(scope.node.hasChildren && scope.node.metaData.noAccess) {
                scope.loadChildren(scope.node);
            }

            var evts = [];

            // Listen for section changes
            evts.push(eventsService.on("appState.sectionState.changed", function(e, args) {
                if (args.key === "currentSection") {
                    //when the section changes disable all delete animations
                    scope.node.deleteAnimations = false;
                }
            }));

            // Update tree icon if changed
            evts.push(eventsService.on("editors.tree.icon.changed", function (e, args) {          
                if (args.icon !== scope.node.icon && args.id === scope.node.id) {
                    scope.node.icon = args.icon;
                }
            }));

            /** Depending on if any menu is shown and if the menu is shown for the current node, toggle delete animations */
            function toggleDeleteAnimations() {
                //if both are false then remove animations
                var hide = !appState.getMenuState("showMenuDialog") && !appState.getMenuState("showMenu");
                if (hide) {
                    scope.node.deleteAnimations = false;
                }
                else {
                    //enable animations for this node if it is the node currently showing a context menu
                    var currentNode = appState.getMenuState("currentNode");
                    if (currentNode && currentNode.id == scope.node.id) {
                        scope.node.deleteAnimations = true;
                    }
                    else {
                        scope.node.deleteAnimations = false;
                    }
                }
            }

            //listen for context menu and current node changes
            evts.push(eventsService.on("appState.menuState.changed", function(e, args) {
                if (args.key === "showMenuDialog" || args.key == "showMenu" || args.key == "currentNode") {
                    toggleDeleteAnimations();
                }
            }));

            //cleanup events
            scope.$on('$destroy', function() {
                for (var e in evts) {
                    eventsService.unsubscribe(evts[e]);
                }
            });
        }
    };
});
