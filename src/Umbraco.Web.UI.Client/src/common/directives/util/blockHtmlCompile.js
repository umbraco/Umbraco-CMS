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
      link: function (scope, element, attrs) {
        function compileBlock(value, blockObject) {
          // In case value is a TrustedValueHolderType, sometimes it
          // needs to be explicitly called into a string in order to
          // get the HTML string.
          element.html(value && value.toString());

          var labelVars = Object.assign(
            blockObject.interpolatableData,
            blockObject.data
          );

          var compileScope = scope.$new(true);
          compileScope = Object.assign(compileScope, labelVars);

          $compile(element.contents())(compileScope);
        }

        // Watch for changes to the isolated block scope
        scope.$watchCollection(function () {
          return scope.$eval(attrs.blockHtmlScope);
        }, function (value) {
          compileBlock(scope.$eval(attrs.blockHtmlCompile), value);
        });

        // Compile the block on initial load
        var ensureCompileRunsOnce = scope.$watch(
          function () {
            return scope.$eval(attrs.blockHtmlCompile);
          },
          function (value) {
            compileBlock(value, scope.$eval(attrs.blockHtmlScope));

            // Use un-watch feature to ensure compilation happens only once.
            ensureCompileRunsOnce();
          }
        );
      }
    };
  });
