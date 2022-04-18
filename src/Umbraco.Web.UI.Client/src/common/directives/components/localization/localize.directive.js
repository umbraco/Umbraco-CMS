angular.module('umbraco.directives')
/**
* @ngdoc directive
* @name umbraco.directives.directive:localize
* @restrict EA
* @function
* @description
* <div>
*   Used to localize text in HTMl-elements or attributes using translation keys. Translations are stored in umbraco/config/lang/ or the /lang-folder of a package i App_Plugins.
* </div>
* <div>
*   <strong>Component/Element</strong><br />
    *   Localize a specific token to put into the HTML as an item
* </div>
* <div>
*   <strong>Attribute</strong><br />
    *   Add a HTML attribute to an element containing the HTML attribute name you wish to localise
    *   Using the format of '@section_key' or 'section_key'
* </div>
* ##Basic Usage
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
* ##Use with tokens
* Also supports tokens inside the translation key, example of a translation
* <pre>
*   <key alias="characters_left">You have %0% characters left of %1%</key>
* </pre>
* Can be used like this:
* <pre>
*   <localize key="general_characters_left" tokens="[5,100]" watch-tokens="true">You have %0% characters left of %1%</localize>
*   <!-- Renders: You have 5 characters left of 100 -->
* </pre>
* Where the "tokens"-attribute is an array of tokens for the translations, "watch-tokens" will make the component watch the expression passed.
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

                // A render function to be able to update tokens as values update.
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
                //Support one or more attribute properties to update
            var keys = attrs.localize.split(',');

            Utilities.forEach(keys, (value, key) => {
                var attr = element.attr(value);
                if (attr) {
                        // Localizing is done async, so make sure the key isn't visible
                        element.removeAttr(value);

                    if (attr[0] === '@') {
                        //If the translation key starts with @ then remove it
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
