angular.module("umbraco.directives")
    .directive('umbAutoResize', function($timeout) {

        return function(scope, element, attr){
            var domEl = element[0];
            var update = function(force) {

                if(force === true){
                    element.height(0);
                }

                if(domEl.scrollHeight !== domEl.clientHeight){
                    element.height(domEl.scrollHeight);
                }
            };

            element.bind('keyup keydown keypress change', update);
            element.bind('blur', function(){ update(true); });

            $timeout(function() {
                update();
            }, 100);


            //I hate bootstrap tabs
            $('a[data-toggle="tab"]').on('shown', update);

            scope.$on('$destroy', function() {
                $('a[data-toggle="tab"]').unbind("shown", update);
            });
    };
});
