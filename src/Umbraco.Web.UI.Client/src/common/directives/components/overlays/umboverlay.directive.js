/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
angular.module("umbraco.directives")
    .directive('umbOverlay', function ($timeout, formHelper) {
        return {

            scope: {
                model: "=",
                view: "=",
                position: "@",
                animation: "@",
                shadow: "@"
            },

            transclude: true,
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/overlays/umb-overlay.html',
            link: function(scope, element, attrs) {

                function activate() {
                  var cssClass = "umb-overlay-center";
                  if(scope.position)
                  {
                      cssClass = "umb-overlay-" + scope.position;
                  }

                  if(scope.animation){
                      cssClass += " " + scope.animation;
                  }

                  var shadow = "shadow-depth-3";
                  if(scope.shadow){
                      shadow =  "shadow-depth-" + scope.shadow;
                  }
                  cssClass += " " + shadow;

                  scope.overlayCssClass = cssClass;
                }

                function setTargetPosition() {

                    var viewportWidth = null;
                    var viewportHeight = null;
                    var mousePositionClickX = null;
                    var mousePositionClickY = null;
                    var elementHeight = null;
                    var elementWidth = null;

                    var position = {
                        right: "inherit",
                        left: "inherit",
                        top: "inherit",
                        bottom: "inherit"
                    };

                    // if mouse click position is know place element with mouse in center
                    if(scope.model.event.pageX && scope.model.event.pageY) {

                      // viewport size
                      viewportWidth = $(window).innerWidth();
                      viewportHeight = $(window).innerHeight();

                      // click position
                      mousePositionClickX = scope.model.event.pageX;
                      mousePositionClickY = scope.model.event.pageY;

                      // element size
                      elementHeight = element.context.clientHeight;
                      elementWidth = element.context.clientWidth;

                      // move element to this position
                      position.left = mousePositionClickX - (elementWidth / 2);
                      position.top = mousePositionClickY - (elementHeight / 2);

                      // check to see if element is outside screen
                      // outside right
                      if( position.left + elementWidth > viewportWidth) {
                          position.right = 0;
                          position.left = "inherit";
                      }

                      // outside bottom
                      if( position.top + elementHeight > viewportHeight ) {
                          position.bottom = 0;
                          position.top = "inherit";
                      }

                      element.css(position);

                    // else change overlay to center position
                    } else {

                      scope.position = "center";
                      activate();

                    }

                }

                activate();

                $timeout(function() {
                    if(scope.position === "target") {
                        setTargetPosition();
                    }
                });

                scope.submitForm = function(model) {

                  if (formHelper.submitForm({ scope: scope })) {

                      formHelper.resetForm({ scope: scope });

                      scope.model.submit(model);

                  }

                };

                scope.closeOverLay = function(){
                    if(scope.model.close){
                        scope.model.close(scope.model);
                    }else{
                        scope.model = null;
                    }
                };

            }


        };
    });
