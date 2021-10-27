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
    <umb-icon icon="icon-alert" class="another-class"></umb-icon>
</pre>
@example
 **/

(function () {
    "use strict";

    function UmbIconDirective(iconHelper) {

        var directive = {
            replace: true,
            transclude: true,
            templateUrl: "views/components/umb-icon.html",
            scope: {
                icon: "@",
                svgString: "=?"
            },
            link: function (scope, element) {

                if (scope.svgString === undefined && scope.svgString !== null && scope.icon !== undefined && scope.icon !== null) {
                    const observer = new IntersectionObserver(_lazyRequestIcon, {rootMargin: "100px"});
                    const iconEl = element[0];

                    observer.observe(iconEl);

                    // make sure to disconnect the observer when the scope is destroyed
                    scope.$on('$destroy', function () {
                        observer.disconnect();
                    });
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

                function _lazyRequestIcon(entries, observer) {
                    entries.forEach(entry => {
                        if (entry.isIntersecting === true) {
                            observer.disconnect();

                            var icon = scope.icon.split(" ")[0]; // Ensure that only the first part of the icon is used as sometimes the color is added too, e.g. see umbeditorheader.directive scope.openIconPicker

                            _requestIcon(icon);
                        }
                    });
                }

                function _requestIcon(icon) {
                    // Reset svg string before requesting new icon.
                    scope.svgString = null;

                    iconHelper.getIcon(icon)
                        .then(data => {
                            if (data && data.svgString) {
                                // Watch source SVG string
                                scope.svgString = data.svgString;
                            }
                        });
                }
            }

        };

        return directive;
    }

    angular.module("umbraco.directives").directive("umbIcon", UmbIconDirective);

})();
