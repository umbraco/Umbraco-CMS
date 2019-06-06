/**
@ngdoc directive
@name umbraco.directives.directive:umbIcon
@restrict E
@scope
@description
Use this directive to show an render an umbraco backoffice svg icon. All svg icons used by this directive should use the following naming convention to keep things consistent: icon-[name of icon]. For example <pre>icon-alert.svg</pre>

<h3>Markup example</h3>

Simple icon
<pre>
    <umb-icon icon-name="icon-alert" />
</pre>

Icon with additional attribute. It can be treated like any other dom element
<pre>
    <umb-icon icon-name="icon-alert" class="icon-class" ng-click="doSomething()" />
</pre>

Manual svg string
This format is only used in the iconpicker.html
<pre>
    <umb-icon 
        icon-name="icon-alert"
        svg-string='<svg height="50" width="50"><circle cx="25" cy="25" r="25" fill="red" /></svg>'
        />
</pre>
@example
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
            },

            link: function (scope) {
                if (scope.svgString === undefined) {
                    var iconName = scope.iconName.split(' ')[0]; // Ensure that only the first part of the iconName is used as sometimes the color is added too, e.g. see umbeditorheader.directive scope.openIconPicker

                    _requestIcon(iconName);
                }
                scope.$watch('iconName', function (newValue, oldValue) {
                    if (newValue && oldValue) {
                        var newIconName = newValue.split(' ')[0];
                        var oldIconName = oldValue.split(' ')[0];

                        if (newIconName !== oldIconName) {
                            _requestIcon(newIconName);
                        }
                    }
                });

                function _requestIcon(iconName) {
                    iconHelper.getIcon(iconName)
                        .then(icon => {
                            if (icon !== null && icon.svgString !== undefined) {
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
