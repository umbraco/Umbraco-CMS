/**
@ngdoc directive
@name umbraco.directives.directive:umbEditorContainer
@restrict E

@description
Use this directive to construct a main content area inside the main editor window.

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
    <li>{@link umbraco.directives.directive:umbEditorView umbEditorView}</li>
    <li>{@link umbraco.directives.directive:umbEditorHeader umbEditorHeader}</li>
    <li>{@link umbraco.directives.directive:umbEditorFooter umbEditorFooter}</li>
</ul>
**/

(function () {
    'use strict';

    function EditorContainerDirective(overlayHelper) {

        function link(scope, el, attr, ctrl) {

            scope.numberOfOverlays = 0;

            // TODO: this shouldn't be a watch, this should be based on an event handler
            scope.$watch(function () {
                return overlayHelper.getNumberOfOverlays();
            }, function (newValue) {
                scope.numberOfOverlays = newValue;
            });

        }

        var directive = {
            transclude: true,
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-container.html',
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbEditorContainer', EditorContainerDirective);

})();
