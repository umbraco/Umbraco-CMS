angular.module('umbraco.directives')
    /**
    * @ngdoc directive
    * @name umbraco.directives.directive:localize
    * @restrict EA
    * @function
    * @description
    * <div>
    *   <strong>Component</strong><br />
    *   Localize a specific token to put into the HTML as an item.
    * </div>
    * <div>
    *   <strong>Attribute</strong><br />
    *   Add an HTML attribute to an element containing the HTML attribute name you wish to localize,
    *   using the format of '@section_key' or 'section_key'.
    * </div>
    * ##Usage
    * <pre>
    * <!-- Component -->
    * <localize key="general_close">Close</localize>
    * <localize key="section_key">Fallback value</localize>
    *
    * <!-- Attribute -->
    * <input type="text" localize="placeholder" placeholder="@placeholders_entername" />
    * <input type="text" localize="placeholder,title" title="@section_key" placeholder="@placeholders_entername" />
    * <div localize="title" title="@section_key"></div>
    * </pre>
    **/
    .directive('localize', function ($log, localizationService) {
        return {
            restrict: 'E',
            scope: {
                key: '@',
                tokens: '=',
                watchTokens: '@'
            },
            replace: true,
            link: function (scope, element, attrs) {
                var key = scope.key;
                scope.text = '';

                // A render function to be able to update tokens as values update
                function render() {
                    element.html(localizationService.tokenReplace(scope.text, scope.tokens || null));
                }

                // As per component definition in ngdoc above, the initial inner html of the element is to be used as fallback value
                var fallbackValue = element.html();
                localizationService.localize(key, null, fallbackValue).then(function (value) {
                    scope.text = value;
                    render();
                });

                if (scope.watchTokens === 'true') {
                    scope.$watch("tokens", render, true);
                }
            }
        };
    })
    .directive('localize', function ($log, localizationService) {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                // Support one or more attribute properties to update
                var keys = attrs.localize.split(',');

                Utilities.forEach(keys, (value, key) => {
                    var attr = element.attr(value);
                    if (attr) {
                        // Localizing is done async, so make sure the key isn't visible
                        element.removeAttr(value);
                        
                        if (attr[0] === '@') {
                            // If the translation key starts with @ then remove it
                            attr = attr.substring(1);
                        }

                        var t = localizationService.tokenize(attr, scope);

                        localizationService.localize(t.key, t.tokens).then(function (val) {
                            element.attr(value, val);
                        });
                    }
                });
            }
        };
    });
