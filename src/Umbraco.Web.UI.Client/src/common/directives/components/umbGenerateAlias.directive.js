angular.module("umbraco.directives")
    .directive('umbGenerateAlias', function ($timeout, entityResource) {
        return {
            restrict: 'E',
            templateUrl: 'views/components/umb-generate-alias.html',
            replace: true,
            scope: {
                alias: '=',
                aliasFrom: '=',
                enableLock: '=?',
                serverValidationField: '@'
            },
            link: function (scope, element, attrs, ctrl) {

                var eventBindings = [];
                var bindWatcher = true;
                var generateAliasTimeout = "";
                var updateAlias = false;

                scope.locked = true;
                scope.placeholderText = "Enter alias...";

                function generateAlias(value) {

                  if (generateAliasTimeout) {
                    $timeout.cancel(generateAliasTimeout);
                  }

                  if( value !== undefined && value !== "" && value !== null) {

                    scope.alias = "Generating Alias...";

                    generateAliasTimeout = $timeout(function () {
                       updateAlias = true;
                        entityResource.getSafeAlias(value, true).then(function (safeAlias) {
                            if (updateAlias) {
                              scope.alias = safeAlias.alias;
                           }
                      });
                    }, 500);

                  } else {
                    updateAlias = true;
                    scope.alias = "";
                    scope.placeholderText = "Enter alias...";
                  }

                }

                // if alias gets unlocked - stop watching alias
                eventBindings.push(scope.$watch('locked', function(newValue, oldValue){
                    if(newValue === false) {
                       bindWatcher = false;
                    }
                }));

                // validate custom entered alias
                eventBindings.push(scope.$watch('alias', function(newValue, oldValue){

                  if(scope.alias === "" && bindWatcher === true || scope.alias === null && bindWatcher === true) {
                    // add watcher
                    eventBindings.push(scope.$watch('aliasFrom', function(newValue, oldValue) {
                       if(bindWatcher) {
                          generateAlias(newValue);
                       }
                    }));
                  }

               }));

               // clean up
               scope.$on('$destroy', function(){
                 // unbind watchers
                 for(var e in eventBindings) {
                   eventBindings[e]();
                  }
               });

            }
        };
    });
