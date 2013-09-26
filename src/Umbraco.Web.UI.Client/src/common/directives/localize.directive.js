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
            if(key[0] === '#')
            {
                key = key.slice(1);
            }

            var value = localizationService.getLocalizedString(key);
            element.html(value);
        }
    };
});