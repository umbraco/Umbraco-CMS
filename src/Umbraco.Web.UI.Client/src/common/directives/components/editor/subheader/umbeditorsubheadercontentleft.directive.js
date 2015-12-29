/**
@ngdoc directive
@name umbraco.directives.directive:umbEditorSubHeaderContentLeft
@restrict E

@description
Use this directive to left align content in a sub header in the main editor window.

<h3>Markup example</h3>
For examples see: {@link umbraco.directives.directive:umbEditorSubHeader umbEditorSubHeader}

<h3>Use in combination with</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbEditorSubHeader umbEditorSubHeader}</li>
    <li>{@link umbraco.directives.directive:umbEditorSubHeaderContentRight umbEditorSubHeaderContentRight}</li>
    <li>{@link umbraco.directives.directive:umbEditorSubHeaderSection umbEditorSubHeaderSection}</li>
</ul>
**/

(function() {
   'use strict';

   function EditorSubHeaderContentLeftDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/subheader/umb-editor-sub-header-content-left.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorSubHeaderContentLeft', EditorSubHeaderContentLeftDirective);

})();
