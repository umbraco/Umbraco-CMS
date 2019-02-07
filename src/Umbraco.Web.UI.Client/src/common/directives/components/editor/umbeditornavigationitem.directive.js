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
        
        var vm = this;
        
        vm.showDropdown = false;
        
        vm.clicked = function() {
            vm.onOpen({item:vm.item});
            
            //vm.mouseOver();// help touch users get the dropdown.
            clearTimeout(vm.mouseOutDelay);
            vm.showDropdown = true;
        };
        
        vm.anchorClicked = function(anchor, $event) {
            
            vm.onOpenAnchor({item:vm.item, anchor:anchor});
            $event.stopPropagation();
            $event.preventDefault();
        };
        
        vm.mouseOver = function() {
            clearTimeout(vm.mouseOutDelay);
            vm.showDropdown = true;
            $scope.$digest();
        }
        
        var hideDropdown = function() {
            vm.showDropdown = false;
            $scope.$digest();
        }
        var hideDropdownBind = hideDropdown.bind(vm);
        
        vm.mouseOut = function() {
            clearTimeout(vm.mouseOutDelay);
            vm.mouseOutDelay = setTimeout(hideDropdownBind, 500);
        }
        
        
        
        var componentNode = $element[0];
        
        componentNode.classList.add('umb-sub-views-nav-item');
        
        componentNode.addEventListener('mouseover', vm.mouseOver.bind(vm));
        componentNode.addEventListener('mouseout', vm.mouseOut.bind(vm));
        
    }
    
    angular
        .module('umbraco.directives.html')
        .component('umbEditorNavigationItem', {
            templateUrl: 'views/components/editor/umb-editor-navigation-item.html',
            controller: UmbEditorNavigationItemController,
            controllerAs: 'vm',
            bindings: {
                item: '=',
                onOpen: '&',
                onOpenAnchor: '&',
                index: '@'
            }
        });

})();
