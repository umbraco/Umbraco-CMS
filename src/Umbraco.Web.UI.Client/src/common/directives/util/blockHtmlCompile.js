/**
* @ngdoc directive
* @name umbraco.directives.directive:blockHtmlCompile
* @element ANY
* @restrict A
* @function
* @description
* Compiles any HTML into the provided scope, e.g. for use with the label field of blocks

* @example
* <example module="umbraco.directives">
*    <file name="index.html">
*        <div block-html-compile="block.label" block-html-scope="block"></div>
*    </file>
* </example>
**/

angular
  .module("umbraco.directives")
  .directive("blockHtmlCompile", function ($compile) {
    return {
      restrict: "A",
      scope: {
        blockHtmlScope: "<",
        blockHtmlCompile: "<"
      },
      link: function (scope, element, attrs) {

        var blockObject = scope.blockHtmlScope;

        function compileBlock() {
          // In case value is a TrustedValueHolderType, sometimes it
          // needs to be explicitly called into a string in order to
          // get the HTML string.
          element.html(scope.blockHtmlCompile);

          var labelVars = {
            $contentTypeName: blockObject.content.contentTypeName,
            $settings: blockObject.settingsData || {},
            $layout: blockObject.layout || {},
            $index: blockObject.index + 1,
            ... blockObject.data
          };

          var compileScope = scope.$new(true);
          compileScope = Object.assign(compileScope, labelVars);

          $compile(element.contents())(compileScope);
        }

        // Watch for changes to the isolated block scope
        scope.$watchCollection(function () {
          return blockObject.data;
        }, compileBlock);
        scope.$watchCollection(function () {
          return blockObject.settingsData;
        }, compileBlock);
        scope.$watchCollection(function () {
          return blockObject.layout;
        }, compileBlock);

        /*
        // No need to watch contentTypeName as it wont change.
        scope.$watch(function () {
          return scope.blockHtmlScope.content.contentTypeName;
        }, compileBlock);
        */

      }
    };
  });
