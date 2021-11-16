/**
@ngdoc directive
@name umbraco.directives.directive:umbBoxHeader
@restrict E
@scope

@description
Use this directive to construct a title. Recommended to use it inside an {@link umbraco.directives.directive:umbBox umbBox} directive. See documentation for {@link umbraco.directives.directive:umbBox umbBox}.

<h3>Markup example</h3>
<pre>
    <umb-box>
        <umb-box-header title="This is a title" description="I can enter a description right here"></umb-box-header>
        <umb-box-content>
            // Content here
        </umb-box-content>
    </umb-box>
</pre>

<h3>Markup example with using titleKey</h3>
<pre>
    <umb-box>
        // the title-key property needs an areaAlias_keyAlias from the language files
        <umb-box-header title-key="areaAlias_keyAlias" description-key="areaAlias_keyAlias"></umb-box-header>
        <umb-box-content>
            // Content here
        </umb-box-content>
    </umb-box>
</pre>
{@link https://our.umbraco.com/documentation/extending/language-files/ Here you can see more about the language files}

<h3>Use in combination with:</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbBox umbBox}</li>
    <li>{@link umbraco.directives.directive:umbBoxContent umbBoxContent}</li>
</ul>

@param {string=} title (<code>attrbute</code>): Custom title text.
@param {string=} titleKey (<code>attrbute</code>): The translation key from the language xml files.
@param {string=} description (<code>attrbute</code>): Custom description text.
@param {string=} descriptionKey (<code>attrbute</code>): The translation key from the language xml files.
**/


(function(){
    'use strict';

    function BoxHeaderDirective(localizationService) {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/html/umb-box/umb-box-header.html',
            scope: {
                titleKey: "@?",
                title: "@?",
                descriptionKey: "@?",
                description: "@?"
            },
            link: function (scope) {

                scope.titleLabel = scope.title;

                if (scope.titleKey) {
                    localizationService.localize(scope.titleKey, [], scope.title).then((data) => {
                        scope.titleLabel = data;
                    });

                }

                scope.descriptionLabel = scope.description;

                if (scope.descriptionKey) {
                    localizationService.localize(scope.descriptionKey, [], scope.description).then((data) => {
                        scope.descriptionLabel = data;
                    });

                }
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbBoxHeader', BoxHeaderDirective);

})();
