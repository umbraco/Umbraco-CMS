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
    
    function UmbEditorNavigationItemController($scope, $element, $attrs) {
        
        console.log("LINKKK!")
        var vm = this;
        
        this.callbackOpen = function(item) {
            console.log("callbackOpen")
            vm.open({item:vm.item});
        };
    }
    
    angular
        .module('umbraco.directives.html')
        .component('umbEditorNavigationItem', {
            templateUrl: 'views/components/editor/umb-editor-navigation-item.html',
            controller: UmbEditorNavigationItemController,
            controllerAs: 'vm',
            bindings: {
                item: '=',
                open: '&',
                index: '@'
            }
        });

})();
