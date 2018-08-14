(function () {
    'use strict';

    function umbVariantStateController($scope, $element) {

        var vm = this;
        
    }

    var umbVariantStateComponent = {
        templateUrl: 'views/components/content/umb-variant-state.html',
        bindings: {
            variant: "<"
        },
        controllerAs: 'vm',
        controller: umbVariantStateController
    };

    angular.module("umbraco.directives")
        .component('umbVariantState', umbVariantStateComponent);

})();
