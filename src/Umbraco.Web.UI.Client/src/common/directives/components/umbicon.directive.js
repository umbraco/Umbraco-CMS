/**
@ngdoc directive
@name umbraco.directives.directive:umbIcon
@restrict E
@scope

@description
Use this directive to show an render an umbraco backoffice svg icon.

<h3>Markup example</h3>
<pre>
    <umb-icon
        name="icon-name"
        request-icon="true" // defaults to false. When true will do a http request to get the svg icon
        >
        // Empty state content
    </umb-icon>
</pre>

**/

(function () {
    'use strict';

    function UmbIconDirective($http, $sce, iconHelper) {

        var directive = {
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-icon.html',
            scope: {
                iconName: '@',
                svgString: '=?',
                requestIcon: '=?'
            },

            link: function(scope) {
                if (scope.requestIcon === undefined) { scope.requestIcon = true; }
                if(scope.requestIcon === true) {
                    var iconName = scope.iconName.split(' ')[0]; // Ensure that only the first part of the iconName is used as sometimes the color is added too, e.g. see umbeditorheader.directive scope.openIconPicker

                    _requestIcon(iconName);
                }
                scope.$watch('iconName', function(newValue, oldValue) {
                    if(newValue && oldValue) {
                        var newIconName = newValue.split(' ')[0];
                        var oldIconName = oldValue.split(' ')[0];

                        if(newIconName !== oldIconName) {
                            _requestIcon(newIconName);
                        }
                    }
                });
                
                function _requestIcon(iconName) {
                    iconHelper.getIcon(iconName)
                    .then(icon => {
                        if(icon !== null && icon.svgString) {
                            scope.svgString = icon.svgString;
                        }
                    });
                }
            }

        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbIcon', UmbIconDirective);

})();
