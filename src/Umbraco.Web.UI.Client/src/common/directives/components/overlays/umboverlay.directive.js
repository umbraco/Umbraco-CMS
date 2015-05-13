/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
angular.module("umbraco.directives")
    .directive('umbOverlay', function () {
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