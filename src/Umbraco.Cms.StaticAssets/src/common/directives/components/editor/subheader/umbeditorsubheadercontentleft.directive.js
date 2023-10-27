/**
@ngdoc directive
@name umbraco.directives.directive:umbEditorSubHeaderContentLeft
@restrict E

@description
Use this directive to left align content in a sub header in the main editor window.

<h3>Markup example</h3>
<pre>
    <div ng-controller="MySection.Controller as vm">

        <form name="mySectionForm" novalidate>

            <umb-editor-view>

                <umb-editor-container>

                    <umb-editor-sub-header>

                        <umb-editor-sub-header-content-left>
                            // left content here
                        </umb-editor-sub-header-content-left>

                        <umb-editor-sub-header-content-right>
                            // right content here
                        </umb-editor-sub-header-content-right>

                    </umb-editor-sub-header>

                </umb-editor-container>

            </umb-editor-view>

        </form>

    </div>
</pre>

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
