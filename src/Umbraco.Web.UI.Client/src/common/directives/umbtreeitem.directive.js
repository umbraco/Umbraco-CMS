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
.directive('umbTreeItem', function ($compile, $http, $templateCache, $interpolate, $log, $location, $rootScope, $window, treeService, $timeout) {
  return {
    restrict: 'E',
    replace: true,

    scope: {
      section: '@',
      cachekey: '@',
      eventhandler: '=',
      currentNode:'=',
      node:'=',
      tree:'='
    },

    template: '<li ng-class="{\'current\': (node == currentNode)}" on-right-click="altSelect(this, node, $event)"><div ng-style="setTreePadding(node)" ng-class="node.stateCssClass" ng-class="{\'loading\': node.loading}" ng-swipe-right="options(this, node, $event)" >' +
        '<ins ng-hide="node.hasChildren" style="width:18px;"></ins>' +        
        '<ins ng-show="node.hasChildren" ng-class="{\'icon-navigation-right\': !node.expanded, \'icon-navigation-down\': node.expanded}" ng-click="load(node)"></ins>' +
        '<i title="#{{node.routePath}}" class="{{node.cssClass}}"></i>' +
        '<a href ng-click="select(this, node, $event)" on-right-click="altSelect(this, node, $event)">{{node.name}}</a>' +
        '<a href class="umb-options" ng-hide="!node.menuUrl" ng-click="options(this, node, $event)"><i></i><i></i><i></i></a>' +
        '<div ng-show="node.loading" class="l"><div></div></div>' +
        '</div>' +
        '</li>',

    link: function (scope, element, attrs) {
        
        //flag to enable/disable delete animations, default for an item is tru
        var deleteAnimations = true;

        /** Helper function to emit tree events */
        function emitEvent(eventName, args) {
          if(scope.eventhandler){
            $(scope.eventhandler).trigger(eventName,args);
          }
        }

        /** updates the node's styles */
        function styleNode(node) {
            node.stateCssClass = (node.cssClasses || []).join(" ");

            if (node.style) {
                $(element).find("i:first").attr("style", node.style);
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

        //add a method to the node which we can use to call to update the node data if we need to ,
        // this is done by sync tree, we don't want to add a $watch for each node as that would be crazy insane slow
        // so we have to do this
        scope.node.updateNodeData = function (newNode) {            
            _.extend(scope.node, newNode);
            //now update the styles
            styleNode(scope.node);
        };

        /**
          Method called when the options button next to a node is called
          In the main tree this opens the menu, but internally the tree doesnt
          know about this, so it simply raises an event to tell the parent controller
          about it.
        */
        scope.options = function(e, n, ev){ 
          emitEvent("treeOptionsClick", {element: e, tree: scope.tree, node: n, event: ev});
        };

        /**
          Method called when an item is clicked in the tree, this passes the 
          DOM element, the tree node object and the original click
          and emits it as a treeNodeSelect element if there is a callback object
          defined on the tree
        */
        scope.select = function(e,n,ev){
            emitEvent("treeNodeSelect", { element: e, tree: scope.tree, node: n, event: ev });
        };

        /**
          Method called when an item is right-clicked in the tree, this passes the 
          DOM element, the tree node object and the original click
          and emits it as a treeNodeSelect element if there is a callback object
          defined on the tree
        */
        scope.altSelect = function(e,n,ev){
            emitEvent("treeNodeAltSelect", { element: e, tree: scope.tree, node: n, event: ev });
        };

        /** method to set the current animation for the node. 
        *  This changes dynamically based on if we are changing sections or just loading normal tree data. 
        *  When changing sections we don't want all of the tree-ndoes to do their 'leave' animations.
        */
        scope.animation = function () {
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
        scope.load = function(node) {
            if (node.expanded) {
                deleteAnimations = false;
                emitEvent("treeNodeCollapsing", {tree: scope.tree, node: node });
                node.expanded = false;
            }
            else {
                scope.loadChildren(node, false);
            }
        };

        /* helper to force reloading children of a tree node */
        scope.loadChildren = function(node, forceReload){
            //emit treeNodeExpanding event, if a callback object is set on the tree
            emitEvent("treeNodeExpanding", { tree: scope.tree, node: node });
            
            if (node.hasChildren && (forceReload || !node.children || (angular.isArray(node.children) && node.children.length === 0))) {
                //get the children from the tree service
                treeService.loadNodeChildren({ node: node, section: scope.section })
                    .then(function(data) {
                        //emit expanded event
                        emitEvent("treeNodeExpanded", {tree: scope.tree, node: node, children: data });
                        enableDeleteAnimations();
                    });
            }
            else {
                emitEvent("treeNodeExpanded", { tree: scope.tree, node: node, children: node.children });
                node.expanded = true;
                enableDeleteAnimations();
            }
        };

        /**
          Helper method for setting correct element padding on tree DOM elements
          Since elements are not children of eachother, we need this indenting done
          manually
        */
        scope.setTreePadding = function(node) {
          return { 'padding-left': (node.level * 20) + "px" };
        };

        //if the current path contains the node id, we will auto-expand the tree item children

        styleNode(scope.node);
        
        var template = '<ul ng-class="{collapsed: !node.expanded}"><umb-tree-item  ng-repeat="child in node.children" eventhandler="eventhandler" tree="tree" current-node="currentNode" node="child" section="{{section}}" ng-animate="animation()"></umb-tree-item></ul>';
        var newElement = angular.element(template);
        $compile(newElement)(scope);
        element.append(newElement);
    }
  };
});
