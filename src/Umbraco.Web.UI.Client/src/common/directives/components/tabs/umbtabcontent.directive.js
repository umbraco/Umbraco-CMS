/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTab
* @restrict E
**/
(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbTabContent', {
            transclude: true,
            templateUrl: 'views/components/tabs/umb-tab-content.html'
        });

})();
