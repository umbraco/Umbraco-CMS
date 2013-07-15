/**
* @ngdoc object 
* @name umbraco.directive:umbTree 
* @restrict E
**/
angular.module("umbraco.directives")
  .directive('umbTree', function ($compile, $log, $q, treeService, notificationsService) {
    
    return {
      restrict: 'E',
      replace: true,
      terminal: false,

      scope: {
        section: '@',
        showoptions: '@',
        showheader: '@',
        cachekey: '@',
        callback: '='
      },

      compile: function (element, attrs) {
         //config
         var hideheader = (attrs.showheader === 'false') ? true : false;
         var hideoptions = (attrs.showoptions === 'false') ? "hide-options" : "";
        
         var template = '<ul class="umb-tree ' + hideoptions + '">' + 
         '<li class="root">';

         if(!hideheader){ 
           template +='<div>' + 
           '<h5><a class="root-link">{{tree.name}}</a></h5>' +
               '<i class="umb-options" ng-hide="tree.root.isContainer" ng-click="options(this, tree.root, $event)"><i></i><i></i><i></i></i>' +
           '</div>';
         }
         template += '<ul>' +
                  '<umb-tree-item ng-repeat="child in tree.root.children" node="child" callback="callback" section="{{section}}" ng-animate="{leave: \'tree-node-delete-leave\'}"></umb-tree-item>' +
                  '</ul>' +
                '</li>' +
               '</ul>';

        var newElem = $(template);
        element.replaceWith(template);

        return function (scope, element, attrs, controller) {

            /** Helper function to emit tree events */
            function emitEvent(eventName, args) {
                if (scope.callback) {
                    $(scope.callback).trigger(eventName, args);
                }
            }

            /**
              Method called when the options button next to the root node is called.
              The tree doesnt know about this, so it raises an event to tell the parent controller
              about it.
            */
            scope.options = function (e, n, ev) {
                emitEvent("treeOptionsClick", { element: e, node: n, event: ev });
            };

            function loadTree() {
                if (scope.section) {

                    //use $q.when because a promise OR raw data might be returned.

                    $q.when(treeService.getTree({ section: scope.section, cachekey: scope.cachekey }))
                        .then(function(data) {
                            //set the data once we have it
                            scope.tree = data;
                        }, function(reason) {
                            notificationsService.error("Tree Error", reason);
                        });
                }
            }


            //watch for section changes
            if(scope.node === undefined){
                scope.$watch("section",function (newVal, oldVal) {
                  if(!newVal){
                    scope.tree = undefined;
                    scope.node = undefined;
                  }else if(newVal !== oldVal){
                    loadTree();
                  }
              });
            }

            //initial change
            loadTree();
         };
       }
      };
    });