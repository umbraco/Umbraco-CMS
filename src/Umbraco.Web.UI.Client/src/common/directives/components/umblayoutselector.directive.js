(function() {
   'use strict';

   function LayoutSelectorDirective() {

      function link(scope, el, attr, ctrl) {

         scope.layoutDropDownIsOpen = false;
         scope.showLayoutSelector = true;

         function activate() {

            setVisibility();

            setActiveLayout(scope.layouts);

         }

         function setVisibility() {

            var numberOfAllowedLayouts = getNumberOfAllowedLayouts(scope.layouts);

            if(numberOfAllowedLayouts === 1) {
               scope.showLayoutSelector = false;
            }

         }

         function getNumberOfAllowedLayouts(layouts) {

            var allowedLayouts = 0;

            for (var i = 0; layouts.length > i; i++) {

               var layout = layouts[i];

               if(layout.selected === true) {
                  allowedLayouts++;
               }

            }

            return allowedLayouts;
         }

         function setActiveLayout(layouts) {

            for (var i = 0; layouts.length > i; i++) {
               var layout = layouts[i];
               if(layout.path === scope.activeLayout.path) {
                  layout.active = true;
               }
            }

         }

         scope.pickLayout = function(selectedLayout) {
             if(scope.onLayoutSelect) {
                 scope.onLayoutSelect(selectedLayout);
                 scope.layoutDropDownIsOpen = false;
             }
         };

         scope.toggleLayoutDropdown = function() {
            scope.layoutDropDownIsOpen = !scope.layoutDropDownIsOpen;
         };

         scope.closeLayoutDropdown = function() {
            scope.layoutDropDownIsOpen = false;
         };

         activate();

      }

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/umb-layout-selector.html',
         scope: {
            layouts: '=',
            activeLayout: '=',
            onLayoutSelect: "="
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbLayoutSelector', LayoutSelectorDirective);

})();
