(function () {
    'use strict';

    function UmbBlockGridRenderAreaSlots() {


        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/propertyeditors/blockgrid/umb-block-grid-render-area-slots.html',
            scope: false
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbBlockGridRenderAreaSlots', UmbBlockGridRenderAreaSlots);

})();
