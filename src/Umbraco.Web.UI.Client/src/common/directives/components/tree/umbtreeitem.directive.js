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

        scope: {
            section: '@',
            eventhandler: '=',
            currentNode: '=',
            enablelistviewexpand: '@',
            node: '=',
            tree: '='
        },

        //TODO: Remove more of the binding from this template and move the DOM manipulation to be manually done in the link function,
        // this will greatly improve performance since there's potentially a lot of nodes being rendered = a LOT of watches!

        template: '<li ng-class="{\'current\': (node == currentNode), \'has-children\': node.hasChildren}" on-right-click="altSelect(node, $event)">' +
            '<div ng-class="getNodeCssClass(node)" ng-swipe-right="options(node, $event)" ng-dblclick="load(node)" >' +
            //NOTE: This ins element is used to display the search icon if the node is a container/listview and the tree is currently in dialog
            //'<ins ng-if="tree.enablelistviewsearch && node.metaData.isContainer" class="umb-tree-node-search icon-search" ng-click="searchNode(node, $event)" alt="searchAltText"></ins>' + 
            '<ins ng-class="{\'icon-navigation-right\': !node.expanded || node.metaData.isContainer, \'icon-navigation-down\': node.expanded && !node.metaData.isContainer}" ng-click="load(node)">&nbsp;</ins>' +
            '<i class="icon umb-tree-icon sprTree" ng-click="select(node, $event)"></i>' +
            '<a class="umb-tree-item__label" href="#/{{node.routePath}}" ng-click="select(node, $event)"></a>' +
            //NOTE: These are the 'option' elipses
            '<a class="umb-options" ng-click="options(node, $event)"><i></i><i></i><i></i></a>' +
            '<div ng-show="node.loading" class="l"><div></div></div>' +
            '</div>' +
            '</li>',
        
        link: function (scope, element, attrs) {

            localizationService.localize("general_search").then(function (value) {
                scope.searchAltText = value;
            });

            //flag to enable/disable delete animations, default for an item is true
            var deleteAnimations = true;

            // Helper function to emit tree events
            function emitEvent(eventName, args) {

                if (scope.eventhandler) {
                    $(scope.eventhandler).trigger(eventName, args);
                }
            }

            // updates the node's DOM/styles
            function setupNodeDom(node, tree) {
                
                //get the first div element
                element.children(":first")
                    //set the padding
                    .css("padding-left", (node.level * 20) + "px");

                //toggle visibility of last 'ins' depending on children
                //visibility still ensure the space is "reserved", so both nodes with and without children are aligned.
                
                if (node.hasChildren || node.metaData.isContainer && scope.enablelistviewexpand === "true") {
                    element.find("ins").last().css("visibility", "visible");
                }
                else {
                    element.find("ins").last().css("visibility", "hidden");
                }

                var icon = element.find("i:first");
                icon.addClass(node.cssClass);
                icon.attr("title", node.routePath);

                element.find("a:first").text(node.name);

                if (!node.menuUrl) {
                    element.find("a.umb-options").remove();
                }

                if (node.style) {
                    element.find("i:first").attr("style", node.style);
                }
            }

            //This will deleteAnimations to true after the current digest
            function enableDeleteAnimations() {
                //do timeout so that it re-enables them after this digest
                $timeout(function () {
                    //enable delete animations
                    deleteAnimations = true;
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
                emitEvent("treeOptionsClick", { element: element, tree: scope.tree, node: n, event: ev });
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

                emitEvent("treeNodeSelect", { element: element, tree: scope.tree, node: n, event: ev });
                ev.preventDefault();
            };

            /**
              Method called when an item is right-clicked in the tree, this passes the 
              DOM element, the tree node object and the original click
              and emits it as a treeNodeSelect element if there is a callback object
              defined on the tree
            */
            scope.altSelect = function (n, ev) {
                emitEvent("treeNodeAltSelect", { element: element, tree: scope.tree, node: n, event: ev });
            };

            /** method to set the current animation for the node. 
            *  This changes dynamically based on if we are changing sections or just loading normal tree data. 
            *  When changing sections we don't want all of the tree-ndoes to do their 'leave' animations.
            */
            scope.animation = function () {
                if (scope.node.showHideAnimation) {
                    return scope.node.showHideAnimation;
                }
                if (deleteAnimations && scope.node.expanded) {
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
                    deleteAnimations = false;
                    emitEvent("treeNodeCollapsing", { tree: scope.tree, node: node, element: element });
                    node.expanded = false;
                }
                else {
                    scope.loadChildren(node, false);
                }
            };

            /* helper to force reloading children of a tree node */
            scope.loadChildren = function (node, forceReload) {
                //emit treeNodeExpanding event, if a callback object is set on the tree
                emitEvent("treeNodeExpanding", { tree: scope.tree, node: node });

                if (node.hasChildren && (forceReload || !node.children || (angular.isArray(node.children) && node.children.length === 0))) {
                    //get the children from the tree service
                    treeService.loadNodeChildren({ node: node, section: scope.section })
                        .then(function (data) {
                            //emit expanded event
                            emitEvent("treeNodeExpanded", { tree: scope.tree, node: node, children: data });
                            enableDeleteAnimations();
                        });
                }
                else {
                    emitEvent("treeNodeExpanded", { tree: scope.tree, node: node, children: node.children });
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

            var template = '<ul ng-class="{collapsed: !node.expanded}"><umb-tree-item  ng-repeat="child in node.children" enablelistviewexpand="{{enablelistviewexpand}}" eventhandler="eventhandler" tree="tree" current-node="currentNode" node="child" section="{{section}}" ng-animate="animation()"></umb-tree-item></ul>';
            var newElement = angular.element(template);
            $compile(newElement)(scope);
            element.append(newElement);

        }
    };
});
