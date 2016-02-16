/**
@ngdoc directive
@name umbraco.directives.directive:umbEditorFooter
@restrict E

@description
Use this directive to construct a footer inside the main editor window.

<h3>Markup example</h3>
<pre>
    <div ng-controller="MySection.Controller as vm">

        <form name="mySectionForm" novalidate>

            <umb-editor-view>

                <umb-editor-header
                    // header configuration>
                </umb-editor-header>

                <umb-editor-container>
                    // main content here
                </umb-editor-container>

                <umb-editor-footer>
                    // footer content here
                </umb-editor-footer>

            </umb-editor-view>

        </form>

    </div>
</pre>

<h3>Use in combination with</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbEditorView umbEditorView}</li>
    <li>{@link umbraco.directives.directive:umbEditorHeader umbEditorHeader}</li>
    <li>{@link umbraco.directives.directive:umbEditorContainer umbEditorContainer}</li>
    <li>{@link umbraco.directives.directive:umbEditorFooterContentLeft umbEditorFooterContentLeft}</li>
    <li>{@link umbraco.directives.directive:umbEditorFooterContentRight umbEditorFooterContentRight}</li>
</ul>
**/

(function() {
   'use strict';

   function EditorFooterDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-footer.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorFooter', EditorFooterDirective);

})();
