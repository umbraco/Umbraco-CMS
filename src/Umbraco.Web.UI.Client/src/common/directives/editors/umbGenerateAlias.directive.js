angular.module("umbraco.directives")
    .directive('umbGenerateAlias', function ($timeout, entityResource) {
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
                var bindWatcher = true;
                var generateAliasTimeout = "";

                scope.locked = true;
                scope.placeholderText = "Enter alias...";

                function generateAlias(value) {

                  if (generateAliasTimeout) {
                    $timeout.cancel(generateAliasTimeout);
                  }

                  if( value !== undefined && value !== "") {

                    scope.alias = "Generating Alias...";

                    generateAliasTimeout = $timeout(function () {
                        entityResource.getSafeAlias(value, true).then(function (safeAlias) {
                        scope.alias = safeAlias.alias;
                      });
                    }, 500);

                  } else {
                    scope.alias = "";
                    scope.placeholderText = "Enter alias...";
                  }

                }

                // if alias gets unlocked - stop watching alias
                scope.$watch('locked', function(newValue, oldValue){
                    if(newValue === false) {
                        unbindWatcher();
                        bindWatcher = false;
                    }
                });

                // validate custom entered alias
                scope.$watch('alias', function(newValue, oldValue){

                  if(scope.alias === "" && bindWatcher === true || scope.alias === null && bindWatcher === true) {
                    // add watcher
                    unbindWatcher = scope.$watch('aliasFrom', function(newValue, oldValue) {
                      if (newValue !== undefined && newValue !== null) {
                        generateAlias(newValue);
                      }
                    });
                  }

                });

            }
        };
    });
