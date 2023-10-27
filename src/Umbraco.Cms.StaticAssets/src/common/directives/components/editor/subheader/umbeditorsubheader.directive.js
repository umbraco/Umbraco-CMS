/**
@ngdoc directive
@name umbraco.directives.directive:umbEditorSubHeader
@restrict E

@description
Use this directive to construct a sub header in the main editor window.
The sub header is sticky and will follow along down the page when scrolling.

<h3>Markup example</h3>
<pre>
    <div ng-controller="MySection.Controller as vm">

        <form name="mySectionForm" novalidate>

            <umb-editor-view>

                <umb-editor-container>

                    <umb-editor-sub-header>
                        // sub header content here
                    </umb-editor-sub-header>

                </umb-editor-container>

            </umb-editor-view>

        </form>

    </div>
</pre>

<h3>Use in combination with</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbEditorSubHeaderContentLeft umbEditorSubHeaderContentLeft}</li>
    <li>{@link umbraco.directives.directive:umbEditorSubHeaderContentRight umbEditorSubHeaderContentRight}</li>
    <li>{@link umbraco.directives.directive:umbEditorSubHeaderSection umbEditorSubHeaderSection}</li>
</ul>
**/

(function() {
   'use strict';

   function EditorSubHeaderDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         scope: {
             "appearance": "@?"
         },
         templateUrl: 'views/components/editor/subheader/umb-editor-sub-header.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorSubHeader', EditorSubHeaderDirective);

})();
