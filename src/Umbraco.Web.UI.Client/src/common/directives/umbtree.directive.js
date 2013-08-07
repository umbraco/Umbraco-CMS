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
           '<h5><a class="root-link">{{tree.name}}</a></h5>' +
               '<i class="umb-options" ng-hide="tree.root.isContainer || !tree.root.menuUrl" ng-click="options(this, tree.root, $event)"><i></i><i></i><i></i></i>' +
           '</div>';
         }
         template += '<ul>' +
                  '<umb-tree-item ng-repeat="child in tree.root.children" eventhandler="eventhandler" node="child" section="{{section}}" ng-animate="animation()"></umb-tree-item>' +
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

            //flag to enable/disable delete animations
            var enableDeleteAnimations = false;

            /** Helper function to emit tree events */
            function emitEvent(eventName, args) {

              if (scope.eventhandler) {
                $(scope.eventhandler).trigger(eventName, args);
              }
             //   $rootScope.$broadcast(eventName, args);
            }

            /** Method to load in the tree data */
            function loadTree() {                
                if (scope.section) {

                    //anytime we want to load the tree we need to disable the delete animations
                    enableDeleteAnimations = false;

                    //use $q.when because a promise OR raw data might be returned.
                    $q.when(treeService.getTree({ section: scope.section, cachekey: scope.cachekey }))
                        .then(function (data) {
                            //set the data once we have it
                            scope.tree = data;

                            //do timeout so that it re-enables them after this digest
                            $timeout(function() {
                                //enable delete animations
                                enableDeleteAnimations = true;
                            });

                        }, function (reason) {
                            notificationsService.error("Tree Error", reason);
                        });
                }
            }

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
              Method called when the options button next to the root node is called.
              The tree doesnt know about this, so it raises an event to tell the parent controller
              about it.
            */
            scope.options = function (e, n, ev) {
                emitEvent("treeOptionsClick", { element: e, node: n, event: ev });
            };
            
            //watch for section changes
            scope.$watch("section", function (newVal, oldVal) {
                if (!newVal) {
                    //store the last section loaded
                    lastSection = oldVal;
                }                
                else if (newVal !== oldVal && newVal !== lastSection) {
                    //only reload the tree data and Dom if the newval is different from the old one
                    // and if the last section loaded is different from the requested one.
                    loadTree();
                    
                    //store the new section to be loaded as the last section
                    lastSection = newVal;
                }
            });

            //initial change
            loadTree();
         };
       }
      };
    });