(function () {
    'use strict';


    // Utils:

    function getInterpolatedIndexOfPositionInWeightMap(target, weights) {
        const map = [0];
        weights.reduce((a, b, i) => { return map[i+1] = a+b; }, 0);
        const foundValue = map.reduce((a, b) => {
            let aDiff = Math.abs(a - target);
            let bDiff = Math.abs(b - target);
    
            if (aDiff === bDiff) {
                return a < b ? a : b;
            } else {
                return bDiff < aDiff ? b : a;
            }
        })
        const foundIndex = map.indexOf(foundValue);
        const targetDiff = (target-foundValue);
        let interpolatedIndex = foundIndex;
        if (targetDiff < 0 && foundIndex === 0) {
            // Don't adjust.
        } else if (targetDiff > 0 && foundIndex === map.length-1) {
            // Don't adjust.
        } else {
            const foundInterpolationWeight = weights[targetDiff >= 0 ? foundIndex : foundIndex-1];
            interpolatedIndex += foundInterpolationWeight === 0 ? interpolatedIndex : (targetDiff/foundInterpolationWeight)
        }
        return interpolatedIndex;
    }

    function isWithinRect(x, y, rect, modifier) {
        return (x > rect.left - modifier && x < rect.right + modifier && y > rect.top - modifier && y < rect.bottom + modifier);
    }



    function UmbBlockGridSorter() {

        // scope.config.identifier
        // scope.config.model

        function link(scope, element) {

            const vm = this;

            const containerEl = element[0].closest('.umb-block-grid__layout-container');
            if (!containerEl) {
                console.error("Could not initialize umb block grid sorter.", element[0])
                return;
            }

            element['umbBlockGridSorter:controller'] = () => {
                return vm;
            };

            scope.$on('$destroy', () => {
                // Destroy!
            });
        }

        var directive = {
            restrict: 'A',
            scope: {
                config: '=umbBlockGridSorter'
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbBlockGridSorter', UmbBlockGridSorter);

})();
