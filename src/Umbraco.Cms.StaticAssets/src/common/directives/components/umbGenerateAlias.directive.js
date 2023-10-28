/**
@ngdoc directive
@name umbraco.directives.directive:umbGenerateAlias
@restrict E
@scope

@description
Use this directive to generate a camelCased umbraco alias.
When the aliasFrom value is changed the directive will get a formatted alias from the server and update the alias model. If "enableLock" is set to <code>true</code>
the directive will use {@link umbraco.directives.directive:umbLockedField umbLockedField} to lock and unlock the alias.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <input type="text" ng-model="vm.name" />

        <umb-generate-alias
            enable-lock="true"
            alias-from="vm.name"
            alias="vm.alias">
        </umb-generate-alias>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller() {

            var vm = this;

            vm.name = "";
            vm.alias = "";

        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

@param {string} alias (<code>binding</code>): The model where the alias is bound.
@param {string} aliasFrom (<code>binding</code>): The model to generate the alias from.
@param {string} validationPosition (<code>binding</code>): The position of the validation. Set to <code>'left'</code> or <code>'right'</code>.
@param {boolean=} enableLock (<code>binding</code>): Set to <code>true</code> to add a lock next to the alias from where it can be unlocked and changed.
**/

angular.module("umbraco.directives")
    .directive('umbGenerateAlias', function ($timeout, entityResource, localizationService) {
        return {
            restrict: 'E',
            templateUrl: 'views/components/umb-generate-alias.html',
            replace: true,
            scope: {
                alias: '=',
                aliasFrom: '=',
                enableLock: '=?',
                validationPosition: '=?',
                serverValidationField: '@'
            },
            link: function (scope, element, attrs, ctrl) {

                var eventBindings = [];
                var bindWatcher = true;
                var generateAliasTimeout = "";
                var updateAlias = false;

                scope.locked = true;

                scope.labels = {
                    idle: "Enter alias...",
                    busy: "Generating alias..."
                };
                
                scope.placeholderText = scope.labels.idle;
                
                localizationService.localize('placeholders_enterAlias').then(function (value) {
                    scope.labels.idle = scope.placeholderText = value;
                });

                localizationService.localize('placeholders_generatingAlias').then(function (value) {
                    scope.labels.busy = value;
                });

                function generateAlias(value) {

                  if (generateAliasTimeout) {
                     $timeout.cancel(generateAliasTimeout);
                  }

                  if (value !== undefined && value !== "" && value !== null) {

                    scope.alias = "";
                    scope.placeholderText = scope.labels.busy;

                    generateAliasTimeout = $timeout(function () {
                       updateAlias = true;
                        entityResource.getSafeAlias(value, true).then(function (safeAlias) {
                            if (updateAlias) {
                                scope.alias = safeAlias.alias;
                            }
                            scope.placeholderText = scope.labels.idle;
                      });
                    }, 500);

                  } else {
                    updateAlias = true;
                    scope.alias = "";
                    scope.placeholderText = scope.labels.idle;
                  }
                }

                // if alias gets unlocked - stop watching alias
                eventBindings.push(scope.$watch('locked', function(newValue, oldValue){
                    if(newValue === false) {
                       bindWatcher = false;
                    }
                }));

                // validate custom entered alias
                eventBindings.push(scope.$watch('alias', function (newValue, oldValue) {
                    if (scope.alias === "" || scope.alias === null || scope.alias === undefined) {
                        if (bindWatcher === true) {
                            // add watcher
                            eventBindings.push(scope.$watch('aliasFrom', function (newValue, oldValue) {
                                if (bindWatcher) {
                                    generateAlias(newValue);
                                }
                            }));
                        }
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
