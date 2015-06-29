angular.module("umbraco.directives")
    .directive('umbGenerateAlias', function ($timeout, contentTypeResource) {
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
                var generateAliasTimeout = "";

                scope.locked = true;

                function activate() {
                  // if alias is already filled - do not add wacther
                  if (scope.alias === undefined || scope.alias === "" || scope.alias === null) {

                    unbindWatcher = scope.$watch('aliasFrom', function(newValue, oldValue) {
                      if (newValue !== undefined && newValue !== null) {
                        generateAlias(newValue);
                      }
                    });

                  }
                }

                function generateAlias(value) {

                    var str = value;

                    // replace special characters with spaces
                    str = str.replace(/[^a-zA-Z0-9]/g, ' ');

                    // camel case string
                    str = str.replace(/(?:^\w|[A-Z]|\b\w|\s+)/g, function(match, index) {
                      if (+match === 0) { return "";}
                      return index === 0 ? match.toLowerCase() : match.toUpperCase();
                    });

                    scope.alias = str;

                    // get safe alias from server
                    validateAlias(scope.aliasFrom, scope.alias);

                }

                // if alias gets unlocked - stop watching alias
                scope.$watch('locked', function(newValue, oldValue){

                    if(newValue === false) {
                        unbindWatcher();
                    }

                });

                function validateAlias(value) {

                  if (generateAliasTimeout) {
                    $timeout.cancel(generateAliasTimeout);
                  }

                  if( value !== undefined && value !== "") {

                    generateAliasTimeout = $timeout(function () {
                      contentTypeResource.getSafeAlias(value, true).then(function(safeAlias){
                        scope.alias = safeAlias;
                      });
                    }, 1000);

                  }

                }

                activate();

            }
        };
    });
