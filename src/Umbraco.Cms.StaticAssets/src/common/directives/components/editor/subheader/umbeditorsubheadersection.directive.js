/**
@ngdoc directive
@name umbraco.directives.directive:umbEditorSubHeaderSection
@restrict E

@description
Use this directive to create sections, divided by borders, in a sub header in the main editor window.

<h3>Markup example</h3>
<pre>
    <div ng-controller="MySection.Controller as vm">

        <form name="mySectionForm" novalidate>

            <umb-editor-view>

                <umb-editor-container>

                    <umb-editor-sub-header>

                        <umb-editor-sub-header-content-right>

                            <umb-editor-sub-header-section>
                                // section content here
                            </umb-editor-sub-header-section>

                            <umb-editor-sub-header-section>
                                // section content here
                            </umb-editor-sub-header-section>

                            <umb-editor-sub-header-section>
                                // section content here
                            </umb-editor-sub-header-section>

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
    <li>{@link umbraco.directives.directive:umbEditorSubHeaderContentLeft umbEditorSubHeaderContentLeft}</li>
    <li>{@link umbraco.directives.directive:umbEditorSubHeaderContentRight umbEditorSubHeaderContentRight}</li>
</ul>
**/

(function() {
   'use strict';

   function EditorSubHeaderSectionDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/subheader/umb-editor-sub-header-section.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorSubHeaderSection', EditorSubHeaderSectionDirective);

})();
