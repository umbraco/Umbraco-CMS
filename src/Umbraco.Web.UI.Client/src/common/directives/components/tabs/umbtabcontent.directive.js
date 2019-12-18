/**
@ngdoc directive
@name umbraco.directives.directive:umbTabContent
@restrict E
@scope

@description
Use this directive to render tab content. For an example see: {@link umbraco.directives.directive:umbTabContent umbTabContent}

@param {string=} tab The tab.

**/
(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbTabContent', {
            transclude: true,
            templateUrl: 'views/components/tabs/umb-tab-content.html',
            controllerAs: 'vm',
            bindings: {
                tab: '<'
            }
        });

})();
