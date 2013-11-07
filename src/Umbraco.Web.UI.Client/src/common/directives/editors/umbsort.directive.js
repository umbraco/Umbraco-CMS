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
            
                var cfg = scope.$eval(element.attr('umb-sort')) || {};

                scope.model = ngModel;

                scope.opts = cfg;
                scope.opts.containerSelector= cfg.containerSelector || ".umb-" + cfg.group + "-container",
                scope.opts.nested= cfg.nested || true,
                scope.opts.drop= cfg.drop || true,
                scope.opts.drag= cfg.drag || true,
                scope.opts.clone = cfg.clone || "<li/>";
                scope.opts.mode = cfg.mode || "list";

                scope.opts.itemSelectorFull = $.trim(scope.opts.itemPath + " " + scope.opts.itemSelector);

                /*
                scope.opts.isValidTarget = function(item, container) {
                        if(container.el.is(".umb-" + scope.opts.group + "-container")){
                            return true;
                        }
                        return false;
                     };
                */

                element.addClass("umb-sort");
                element.addClass("umb-" + cfg.group + "-container");

                scope.opts.onDrag = function (item, position)  {
                    if(scope.opts.mode === "list"){
                      item.css({
                            left: position.left - adjustment.left,
                            top: position.top - adjustment.top
                          });  
                    }
                };


                scope.opts.onDrop = function (item, targetContainer, _super)  {
                      
                      if(scope.opts.mode === "list"){
                        //list mode
                        var clonedItem = $(scope.opts.clone).css({height: 0});
                        item.after(clonedItem);
                        clonedItem.animate({'height': item.height()});
                        
                        item.animate(clonedItem.position(), function  () {
                           clonedItem.detach();
                           _super(item);
                        });
                      }

                      var children = $(scope.opts.itemSelectorFull, targetContainer.el);
                      var targetIndex = children.index(item);
                      var targetScope = $(targetContainer.el[0]).scope();
                      

                      if(targetScope === umbSortContextInternal.sourceScope){
                          if(umbSortContextInternal.sourceScope.opts.onSortHandler){
                              var _largs = {
                                oldIndex: umbSortContextInternal.sourceIndex,
                                newIndex: targetIndex,
                                scope: umbSortContextInternal.sourceScope
                              };

                              umbSortContextInternal.sourceScope.opts.onSortHandler.call(this, item, _largs);
                          }
                      }else{
                        

                        if(targetScope.opts.onDropHandler){
                            var args = {
                              sourceScope: umbSortContextInternal.sourceScope,
                              sourceIndex: umbSortContextInternal.sourceIndex,
                              sourceContainer: umbSortContextInternal.sourceContainer,

                              targetScope: targetScope,
                              targetIndex: targetIndex,
                              targetContainer: targetContainer
                            };   

                            targetScope.opts.onDropHandler.call(this, item, args);
                        }

                        if(umbSortContextInternal.sourceScope.opts.onReleaseHandler){
                            var _args = {
                              sourceScope: umbSortContextInternal.sourceScope,
                              sourceIndex: umbSortContextInternal.sourceIndex,
                              sourceContainer: umbSortContextInternal.sourceContainer,

                              targetScope: targetScope,
                              targetIndex: targetIndex,
                              targetContainer: targetContainer
                            };

                            umbSortContextInternal.sourceScope.opts.onReleaseHandler.call(this, item, _args);
                        }
                      }
                };

                scope.changeIndex = function(from, to){
                    scope.$apply(function(){
                      var i = ngModel.$modelValue.splice(from, 1)[0];
                      ngModel.$modelValue.splice(to, 0, i);
                    });
                };

                scope.move = function(args){
                    var from = args.sourceIndex;
                    var to = args.targetIndex;

                    if(args.sourceContainer === args.targetContainer){
                        scope.changeIndex(from, to);
                    }else{
                      scope.$apply(function(){
                        var i = args.sourceScope.model.$modelValue.splice(from, 1)[0];
                        args.targetScope.model.$modelvalue.splice(to,0, i);
                      });
                    }
                };

                scope.opts.onDragStart = function (item, container, _super) {
                      var children = $(scope.opts.itemSelectorFull, container.el);
                      var offset = item.offset();
                      
                      umbSortContextInternal.sourceIndex = children.index(item);
                      umbSortContextInternal.sourceScope = $(container.el[0]).scope();
                      umbSortContextInternal.sourceContainer = container;

                      //current.item = ngModel.$modelValue.splice(current.index, 1)[0];

                      var pointer = container.rootGroup.pointer;
                      adjustment = {
                        left: pointer.left - offset.left,
                        top: pointer.top - offset.top
                      };

                      _super(item, container);
                };
                  
                element.sortable( scope.opts );
             }
          };

        });