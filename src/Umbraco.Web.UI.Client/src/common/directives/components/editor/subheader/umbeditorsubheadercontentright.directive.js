/**
@ngdoc directive
@name umbraco.directives.directive:umbEditorSubHeaderContentRight
@restrict E

@description
Use this directive to rigt align content in a sub header in the main editor window.

<h3>Markup example</h3>
For examples see: {@link umbraco.directives.directive:umbEditorSubHeader umbEditorSubHeader}

<h3>Use in combination with</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbEditorSubHeader umbEditorSubHeader}</li>
    <li>{@link umbraco.directives.directive:umbEditorSubHeaderContentLeft umbEditorSubHeaderContentLeft}</li>
    <li>{@link umbraco.directives.directive:umbEditorSubHeaderSection umbEditorSubHeaderSection}</li>
</ul>
**/

(function() {
   'use strict';

   function EditorSubHeaderContentRightDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/subheader/umb-editor-sub-header-content-right.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorSubHeaderContentRight', EditorSubHeaderContentRightDirective);

})();
