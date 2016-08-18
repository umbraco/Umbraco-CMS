
/**
@ngdoc directive
@name umbraco.directives.directive:umbProgressBar
@restrict E
@scope

@description
Use this directive to generate a progress bar.

<h3>Markup example</h3>
<pre>
    <umb-progress-bar
        percentage="60">
    </umb-progress-bar>
</pre>

@param {number} percentage (<code>attribute</code>): The progress in percentage.
**/

(function() {
    'use strict';

    function ProgressBarDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-progress-bar.html',
            scope: {
                percentage: "@"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbProgressBar', ProgressBarDirective);

})();
