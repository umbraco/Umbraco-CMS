angular.module("umbraco.directives")
    .directive('delayedMouseleave', function ($timeout, $parse) {
        return {
            restrict: 'A',
            link: function (scope, element, attrs, ctrl) {
                var active = false;
                var fn = $parse(attrs.delayedMouseleave);
                element.on("mouseleave", function(event) {
                    var callback = function() {
                        fn(scope, {$event:event});
                    };

                    active = false;
                    $timeout(function(){
                        if(active === false){
                            scope.$apply(callback);
                        }
                    }, 650);
                });

                element.on("mouseenter", function(event, args){
                    active = true;
                });
            }
        };
    });
