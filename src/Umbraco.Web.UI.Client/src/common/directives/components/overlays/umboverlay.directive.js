/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
angular.module("umbraco.directives")
    .directive('umbOverlay', function ($timeout) {
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


                function setTargetPosition() {

                    var position = {
                        right: "inherit",
                        left: "inherit",
                        top: "inherit",
                        bottom: "inherit"
                    };

                    // viewport size
                    var viewportWidth = $(window).innerWidth();
                    var viewportHeight = $(window).innerHeight();

                    // mouse click position
                    var mousePositionClickX = scope.model.event.pageX;
                    var mousePositionClickY = scope.model.event.pageY;

                    // element size
                    var elementHeight = element.context.clientHeight;
                    var elementWidth = element.context.clientWidth;

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


                }


                $timeout(function() {
                    if(scope.position === "target") {
                        setTargetPosition();
                    }
                });

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