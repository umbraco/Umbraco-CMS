/**
 * @ngdoc directive
 * @name umbraco.directives.directive:umbSort
 * @element div
 * @function
 *
 * @description
 * Resize div's automatically to fit to the bottom of the screen, as an optional parameter an y-axis offset can be set
 * So if you only want to scale the div to 70 pixels from the bottom you pass "70"
 *
 * @example
   <example module="umbraco.directives">
     <file name="index.html">
         <div umb-sort="70" class="input-block-level"></div>
     </file>
   </example>
 */
angular.module("umbraco.directives")
  .value('umbSortContextInternal',{})
  .directive('umbSort', function($log,umbSortContextInternal) {
          return {
            require: '?ngModel',
            link: function(scope, element, attrs, ngModel) {
              


              var adjustment;
              var current = {};
                            
             // if(ngModel){

             //   ngModel.$render = function() {

                $log.log(element);
                var cfg = scope.$eval(element.attr('umb-sort')) || {};

                scope.opts = {
                    pullPlaceholder: true,
                    onDrop: null,
                    onDragStart:null,
                    onDrag:null,
                    group: cfg.group,
                    handle: ".handle",
                    containerSelector: cfg.containerSelector || ".umb-" + cfg.group + "-container",
                    nested: cfg.nested || true,
                    drop: cfg.drop || true,
                    drag: cfg.drag || true,
                    isValidTarget: function(item, container) {

                        if(container.el.is(".umb-" + cfg.group + "-container")){
                            $log.log(container);
                            return true;
                        }

                        return false;
                     },
                    events:  cfg
                };

                element.addClass("umb-sort");
                element.addClass("umb-" + cfg.group + "-container");

                scope.opts.onDrag = function (item, position)  {
                    item.css({
                          left: position.left - adjustment.left,
                          top: position.top - adjustment.top
                        });
                };


                scope.opts.onDrop = function (item, targetContainer, _super)  {
                      var children = $("li", targetContainer.el);
                      var targetScope = $(targetContainer.el[0]).scope();
                      var newIndex = children.index(item);

                      if(targetScope.opts.events.onDropHandler){
                          var args = {
                            sourceScope: umbSortContextInternal.startScope,
                            startIndex: umbSortContextInternal.startIndex,
                            startContainer: umbSortContextInternal.startContainer,

                            targetScope: targetScope,
                            targetIndex: newIndex,
                            targetContainer: targetContainer
                          };   

                          targetScope.opts.events.onDropHandler.call(this, item, args);
                      }

                      if(umbSortContextInternal.startScope.opts.events.onReleaseHandler){
                          var _args = {
                            sourceScope: umbSortContextInternal.startScope,
                            startIndex: umbSortContextInternal.startIndex,
                            startContainer: umbSortContextInternal.startContainer,

                            targetScope: targetScope,
                            targetIndex: newIndex,
                            targetContainer: targetContainer
                          };

                          umbSortContextInternal.startScope.opts.events.onReleaseHandler.call(this, item, _args);
                      }

                      var clonedItem = $('<li/>').css({height: 0});
                      item.before(clonedItem);
                      clonedItem.animate({'height': item.height()});
                      
                      scope.$apply(function(){
                         item.animate(clonedItem.position(), function  () {
                           clonedItem.detach();
                           _super(item);
                         });
                      });
                };

                scope.opts.onDragStart = function (item, container, _super) {
                      var children = $("li", container.el);
                      var offset = item.offset();
                      
                      umbSortContextInternal.startIndex = children.index(item);
                      umbSortContextInternal.startScope = $(container.el[0]).scope();
                      umbSortContextInternal.startContainer = container;

                      //current.item = ngModel.$modelValue.splice(current.index, 1)[0];

                      var pointer = container.rootGroup.pointer;
                      adjustment = {
                        left: pointer.left - offset.left,
                        top: pointer.top - offset.top
                      };

                      _super(item, container);
                };
                

                  
                  element.sortable( scope.opts );
              //  };
              
             // }
                // Create sortable
             }
          };

        });