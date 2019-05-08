(function () {
    'use strict';
    
    function UmbEditorNavigationItemController($scope, $element, $attrs) {
        
        
        var componentNode = $element[0];
        componentNode.classList.add('umb-sub-views-nav-item');
        
        var timerId = null;
        
        function openAnchorDropdown() {
            clearTimeout(timerId);
            componentNode.classList.add('--open-anchor-dropdown');
            componentNode.addEventListener('mouseout', requestCloseAnchorDropdown);
        }
        function requestCloseAnchorDropdown() {
            componentNode.removeEventListener('mouseout', requestCloseAnchorDropdown);
            componentNode.addEventListener('mouseover', cancelCloseAnchorDropdown);
            timerId = setTimeout(doCloseAnchorDropdown, 500);
        }
        function cancelCloseAnchorDropdown() {
            clearTimeout(timerId);
            componentNode.removeEventListener('mouseover', cancelCloseAnchorDropdown);
            componentNode.addEventListener('mouseout', requestCloseAnchorDropdown);
        }
        function doCloseAnchorDropdown() {
            componentNode.classList.remove('--open-anchor-dropdown');
        }
        
        
        var vm = this;
        
        vm.clicked = function() {
            if (vm.item.active !== true) {
                vm.onOpen({item:vm.item});
            }
            openAnchorDropdown();
        };
        
        
        
        vm.anchorClicked = function(anchor, $event) {
            vm.onOpenAnchor({item:vm.item, anchor:anchor});
            $event.stopPropagation();
            $event.preventDefault();
        };
        
        $scope.$watch('item.active', function(newValue, oldValue) {
            if(oldValue !== newValue && newValue === false) {
                closeAnchorDropdown();
            }
        })
        
        //ensure to unregister from all dom-events
        $scope.$on('$destroy', function () {
            componentNode.removeEventListener("mouseout", closeAnchorDropdown);
        });
        
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
                hotkey: '<'
            }
        });

})();
