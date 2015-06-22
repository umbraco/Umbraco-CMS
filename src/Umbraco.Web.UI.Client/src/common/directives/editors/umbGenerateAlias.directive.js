angular.module("umbraco.directives")
    .directive('umbGenerateAlias', function ($timeout) {
        return {
            restrict: 'E',
            templateUrl: 'views/components/umb-generate-alias.html',
            replace: true,
            scope: {
                alias: '=',
                aliasFrom: '=',
                enableLock: '=?'
            },
            link: function (scope, element, attrs, ctrl) {

                var unbindWatcher = function(){};

                scope.locked = true;

                function init() {

                    if(scope.alias === undefined || scope.alias === "" || scope.alias === null) {

                        unbindWatcher = scope.$watch('aliasFrom', function (newValue, oldValue) {

                            if(newValue !== undefined && newValue !== null) {
                                generateAlias(newValue);
                            }

                        });

                    }

                }

                function generateAlias(value) {

                    var str = value;

                    // capitalize all words
                    str = str.replace(/\w\S*/g, function(txt){return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();});

                    // remove spaces
                    str = str.replace(/\s/g, '');

                    scope.alias = str;

                }


                scope.$watch('locked', function(newValue, oldValue){

                    if(newValue === false) {
                        unbindWatcher();
                    }
                    
                });

                init();

            }
        };
    });