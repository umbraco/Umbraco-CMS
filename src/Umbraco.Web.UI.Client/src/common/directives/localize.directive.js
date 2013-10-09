angular.module("umbraco.directives")
.directive('localize', function ($log, localizationService) {
    return {
        restrict: 'E',
        scope:{
            key: '@'
        },
        replace: true,
        link: function (scope, element, attrs) {
            var key = scope.key;
            var value = localizationService.localize(key);
            element.html(value);
        }
    };
})
.directive('localize', function ($log, localizationService) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            var keys = attrs.localize.split(',');

            for (var i = keys.length - 1; i >= 0; i--) {
                var attr = element.attr(keys[i]);

                if(attr){
                    var localizer = attr.split(':');
                    var tokens;
                    var key = localizer[0];

                    if(localizer.length > 0){
                        tokens = localizer[1].split(',');
                        for (var x = 0; x < tokens.length; x++) {
                            tokens[x] = scope.$eval(tokens[x]);
                        } 
                    }

                    if(key[0] === '@'){
                        element.attr(keys[i], localizationService.localize(key.substring(1), tokens));    
                    }
                }
            }
        }
    };
});