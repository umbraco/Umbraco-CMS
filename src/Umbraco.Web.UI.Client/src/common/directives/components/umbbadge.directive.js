/**
@ngdoc directive
@name umbraco.directives.directive:umbBadge
@restrict E
@scope

@description
Use this directive to render a badge.

<h3>Markup example</h3>
<pre>
    <div>

        <umb-badge
            size="s"
            color="secondary">
        </umb-badge>

	</div>
</pre>

@param {string} size (<code>attribute</code>): The size (xxs, xs, s, m, l, xl).
@param {string} color (<code>attribute</code>): The color of the highlight (primary, secondary, success, warning, danger, gray).
**/

(function () {
    'use strict';

    function BadgeDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            transclude: true,
            templateUrl: 'views/components/umb-badge.html',
            scope: {
                size: "@?",
                color: "@?"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbBadge', BadgeDirective);

})();
