/**
@ngdoc directive
@name umbraco.directives.directive:umbEmptyState
@restrict E
@scope

@description
Use this directive to show an empty state message.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-empty-state
            ng-if="!vm.items"
            position="center">
            // Empty state content
        </umb-empty-state>

    </div>
</pre>

@param {string=} size Set the size of the text ("small", "large").
@param {string=} position Set the position of the text ("center").
**/

(function() {
   'use strict';

   function EmptyStateDirective() {

      var directive = {
         restrict: 'E',
         replace: true,
         transclude: true,
         templateUrl: 'views/components/umb-empty-state.html',
         scope: {
            size: '@',
            position: '@'
         }
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEmptyState', EmptyStateDirective);

})();
