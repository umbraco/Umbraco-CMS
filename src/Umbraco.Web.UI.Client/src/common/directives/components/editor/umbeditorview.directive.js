/**
@ngdoc directive
@name umbraco.directives.directive:umbEditorView
@restrict E
@scope

@description
Use this directive to construct the main editor window.

<h3>Markup example</h3>
<pre>
    <div ng-controller="Umbraco.Controller as vm">

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

    </div>
</pre>

<h3>Use in combination with</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbEditorHeader umbEditorHeader}</li>
    <li>{@link umbraco.directives.directive:umbEditorContainer umbEditorContainer}</li>
    <li>{@link umbraco.directives.directive:umbEditorFooter umbEditorFooter}</li>
</ul>
**/

(function() {
   'use strict';

   function EditorViewDirective() {

       function link(scope, el, attr) {

           if(attr.footer) {
               scope.footer = attr.footer;
           }

       }

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-view.html',
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorView', EditorViewDirective);

})();
