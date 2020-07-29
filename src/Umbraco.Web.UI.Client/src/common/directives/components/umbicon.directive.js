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
    <umb-icon icon="icon-alert"></umb-icon>
</pre>

Icon with additional attribute. It can be treated like any other dom element
<pre>
    <umb-icon icon="icon-alert" class="icon-class" ng-click="doSomething()"></umb-icon>
</pre>

Manual svg string
This format is only used in the iconpicker.html
<pre>
    <umb-icon 
        icon="icon-alert"
        svg-string='<svg height="50" width="50"><circle cx="25" cy="25" r="25" fill="red" /></svg>'>
    </umb-icon>
</pre>
@example
 **/

(function () {
    "use strict";

    function UmbIconDirective($http, $sce, iconHelper) {

        var directive = {
            replace: true,
            transclude: true,
            templateUrl: "views/components/umb-icon.html",
            scope: {
                icon: "@",
                svgString: "=?"
            },

            link: function (scope) {
                if (scope.svgString === undefined) {
                    var icon = scope.icon.split(" ")[0]; // Ensure that only the first part of the icon is used as sometimes the color is added too, e.g. see umbeditorheader.directive scope.openIconPicker

                    _requestIcon(icon);
                }
                scope.$watch("icon", function (newValue, oldValue) {
                    if (newValue && oldValue) {
                        var newicon = newValue.split(" ")[0];
                        var oldicon = oldValue.split(" ")[0];

                        if (newicon !== oldicon) {
                            _requestIcon(newicon);
                        }
                    }
                });

                function _requestIcon(icon) {
                    iconHelper.getIcon(icon)
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

    angular.module("umbraco.directives").directive("umbIcon", UmbIconDirective);

})();
