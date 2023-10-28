/**
@ngdoc directive
@name umbraco.directives.directive:umbProgressCircle
@restrict E
@scope

@description
Use this directive to render a circular progressbar.

<h3>Markup example</h3>
<pre>
    <div>
    
        <umb-progress-circle
            percentage="80"
            size="60"
            color="secondary">
        </umb-progress-circle>

	</div>
</pre>

@param {string} percentage (<code>attribute</code>): Takes a number between 0 and 100 and applies it to the circle's highlight length.
@param {string} size (<code>attribute</code>): This parameter defines the width and the height of the circle in pixels.
@param {string} color (<code>attribute</code>): The color of the highlight (primary, secondary, success, warning, danger). Success by default. 
**/

(function (){
    'use strict';

    function ProgressCircleDirective($http, $timeout) {

        function link(scope, element, $filter) {
            
            function onInit() {
                            
                // making sure we get the right numbers
                var percent = scope.percentage;

                if (percent > 100) {
                    percent = 100;
                }
                else if (percent < 0) {
                    percent = 0;
                }

                // calculating the circle's highlight
                var circle = element.find(".umb-progress-circle__highlight");
                var r = circle.attr('r');
                var strokeDashArray = (r*Math.PI)*2;

                // Full circle length
                scope.strokeDashArray = strokeDashArray;

                var strokeDashOffsetDifference = (percent/100)*strokeDashArray;
                var strokeDashOffset = strokeDashArray - strokeDashOffsetDifference;

                // Distance for the highlight dash's offset
                scope.strokeDashOffset = strokeDashOffset;

                // set font size
                scope.percentageSize = (scope.size * 0.3) + "px";

            }

            onInit();
        }


        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-progress-circle.html',
            scope: {
                percentage: "@",
                size: "@?",
                color: "@?"
            },
            link: link

        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbProgressCircle', ProgressCircleDirective);

})();
