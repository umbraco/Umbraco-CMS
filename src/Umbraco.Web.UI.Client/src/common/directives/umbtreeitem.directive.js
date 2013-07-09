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
        
        function emitEvent(eventName, args){

          if(scope.callback){
            $(scope.callback).trigger(eventName,args);
          }
        }

        scope.options = function(e, n, ev){ 
          emitEvent("treeOptionsClick", {element: e, node: n, event: ev});
        };

        /**
         * @ngdoc function
         * @name select
         * @methodOf umbraco.directives.umbTreeItem
         * @function
         *
         * @description
         * Handles the click event of a tree node

         * @param n {object} The tree node object associated with the click
         */
        scope.select = function(e,n,ev){
            emitEvent("treeNodeSelect", { element: e, node: n, event: ev });
        };

        scope.load = function (arrow, node) {

          if (node.expanded){
            emitEvent("treeNodeCollapsing", { element: arrow, node: node});

            node.expanded = false;
            node.children = [];
          }else {
            
            emitEvent("treeNodeExpanding", { element: arrow, node: node});

            node.loading = true;

            treeService.getChildren( { node: node, section: scope.section } )
                .then(function (data) {

                    emitEvent("treeNodeLoaded", { element: arrow, node: node, children: data});

                    node.loading = false;
                    node.children = data;
                    node.expanded = true;

                    emitEvent("treeNodeExpanded", { element: arrow, node: node, children: data});

                }, function (reason) {

                    emitEvent("treeNodeLoadError", { element: arrow, node: node, error: reason});

                    node.loading = false;
                    notificationsService.error(reason);
                });
            }   
        };

        scope.setTreePadding = function(node) {
          return { 'padding-left': (node.level * 20) + "px" };
        };

        var template = '<ul ng-class="{collapsed: !node.expanded}"><umb-tree-item ng-repeat="child in node.children" callback="callback" node="child" section="{{section}}"></umb-tree-item></ul>';
        var newElement = angular.element(template);
        $compile(newElement)(scope);
        element.append(newElement);
    }
  };
});