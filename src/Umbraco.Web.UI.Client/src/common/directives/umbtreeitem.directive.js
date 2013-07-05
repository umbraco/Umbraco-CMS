angular.module("umbraco.directives")
.directive('umbTreeItem', function($compile, $http, $templateCache, $interpolate, $log, $location, treeService, notificationsService) {
  return {
    restrict: 'E',
    replace: true,

    scope: {
      section: '@',
      cachekey: '@',
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
      
        scope.options = function(e, n, ev){ 
          scope.$emit("treeOptionsClick", {element: e, node: n, event: ev});
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
            scope.$emit("treeNodeSelect", { element: e, node: n, event: ev });
        };

        scope.load = function (arrow, node) {

          if (node.expanded){
            node.expanded = false;
            node.children = [];
            
          }else {
            
            node.loading = true;

            treeService.getChildren( { node: node, section: scope.section } )
                .then(function (data) {
                    node.loading = false;
                   // $(arrow).parent().remove(loader);
                    
                    node.children = data;
                    node.expanded = true;
                }, function (reason) {
                  //  $(arrow).parent().remove(loader);
                    node.loading = false;
                    notificationsService.error(reason);
                    
                    //alert(reason);
                    return;
                });
            }   
        };

        scope.setTreePadding = function(node) {
          return { 'padding-left': (node.level * 20) + "px" };
        };

        var template = '<ul ng-class="{collapsed: !node.expanded}"><umb-tree-item ng-repeat="child in node.children" node="child" section="{{section}}"></umb-tree-item></ul>';
        var newElement = angular.element(template);
        $compile(newElement)(scope);
        element.append(newElement);
    }
  };
});