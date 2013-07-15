/**
 * @ngdoc object
 * @name umbraco.directives:umbTreeItem
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
   <example module="umbraco.directives">
    <file name="index.html">
         <umb-tree-item ng-repeat="child in tree.children" node="child" callback="callback" section="content"></umb-tree-item>
    </file>
   </example>
 */
angular.module("umbraco.directives")
.directive('umbTreeItem', function($compile, $http, $templateCache, $interpolate, $log, $location, treeService, notificationsService) {
  return {
    restrict: 'E',
    replace: true,

    scope: {
      section: '@',
      cachekey: '@',
      callback: '=',
      node:'='
    },

    template: '<li><div ng-style="setTreePadding(node)" ng-class="{\'loading\': node.loading}">' +
        '<ins ng-hide="node.hasChildren" style="background:none;width:18px;"></ins>' +        
        '<ins ng-show="node.hasChildren" ng-class="{\'icon-caret-right\': !node.expanded, \'icon-caret-down\': node.expanded}" ng-click="load(this, node)"></ins>' +
        '<i class="{{node | umbTreeIconClass:\'icon umb-tree-icon sprTree\'}}" style="{{node | umbTreeIconStyle}}"></i>' +
        '<a ng-click="select(this, node, $event)" >{{node.name}}</a>' +
        '<i class="umb-options" ng-click="options(this, node, $event)"><i></i><i></i><i></i></i>' +
        '<div ng-show="node.loading" class="l"><div></div></div>' +
        '</div>' +
        '</li>',

    link: function (scope, element, attrs) {
        
        //flag to enable/disable delete animations, default for an item is tru
        var enableDeleteAnimations = true;

        /** Helper function to emit tree events */
        function emitEvent(eventName, args){

          if(scope.callback){
            $(scope.callback).trigger(eventName,args);
          }
        }

        /**
          Method called when the options button next to a node is called
          In the main tree this opens the menu, but internally the tree doesnt
          know about this, so it simply raises an event to tell the parent controller
          about it.
        */
        scope.options = function(e, n, ev){ 
          emitEvent("treeOptionsClick", {element: e, node: n, event: ev});
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

        /** method to set the current animation for the node. 
        *  This changes dynamically based on if we are changing sections or just loading normal tree data. 
        *  When changing sections we don't want all of the tree-ndoes to do their 'leave' animations.
        */
        scope.animation = function () {
            if (enableDeleteAnimations) {
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
        scope.load = function(arrow, node) {

            if (node.expanded) {
                enableDeleteAnimations = false;
                emitEvent("treeNodeCollapsing", { element: arrow, node: node });

                node.expanded = false;
                node.children = [];
            }
            else {

                //emit treeNodeExpanding event, if a callback object is set on the tree
                emitEvent("treeNodeExpanding", { element: arrow, node: node });

                //set element state to loading
                node.loading = true;

                //get the children from the tree service
                treeService.getChildren({ node: node, section: scope.section })
                    .then(function(data) {

                        //emit event
                        emitEvent("treeNodeLoaded", { element: arrow, node: node, children: data });

                        //set state to done and expand
                        node.loading = false;
                        node.children = data;
                        node.expanded = true;

                        //emit expanded event
                        emitEvent("treeNodeExpanded", { element: arrow, node: node, children: data });

                    }, function(reason) {

                        //in case of error, emit event
                        emitEvent("treeNodeLoadError", { element: arrow, node: node, error: reason });

                        //stop show the loading indicator  
                        node.loading = false;

                        //tell notications about the error
                        notificationsService.error(reason);
                    });
                
                enableDeleteAnimations = true;
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
        
        var template = '<ul ng-class="{collapsed: !node.expanded}"><umb-tree-item ng-repeat="child in node.children" callback="callback" node="child" section="{{section}}" ng-animate="animation()"></umb-tree-item></ul>';
        var newElement = angular.element(template);
        $compile(newElement)(scope);
        element.append(newElement);
    }
  };
});
