(function () {
    'use strict';
    
    function UmbEditorNavigationItemController($scope, $element, $attrs) {
        
        var vm = this;

        vm.close = function () {
            vm.expanded = false;
        };

        vm.clicked = function () {
            vm.expanded = vm.item.anchors && vm.item.anchors.length > 1 && !vm.expanded;
            vm.onOpen({item:vm.item});
        };
        
        vm.anchorClicked = function(anchor, $event) {
            vm.onOpenAnchor({item:vm.item, anchor:anchor});
            $event.stopPropagation();
            $event.preventDefault();
        };
        
        // needed to make sure that we update what anchors are active.
        vm.mouseOver = function() {
            $scope.$digest();
        }
        
        var componentNode = $element[0];
        
        componentNode.classList.add('umb-sub-views-nav-item');
        componentNode.addEventListener('mouseover', vm.mouseOver);

        //ensure to unregister from all dom-events
        $scope.$on('$destroy', function () {
            componentNode.removeEventListener("mouseover", vm.mouseOver);
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
