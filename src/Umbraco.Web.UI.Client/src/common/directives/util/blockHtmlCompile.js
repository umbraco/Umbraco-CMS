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
      link: function (scope, element) {

        var blockObject = scope.blockHtmlScope;
        var labelElement = $('<div></div>', { text: scope.blockHtmlCompile});

        element.append(labelElement)

        var observer = new MutationObserver(function(mutations) {
          mutations.forEach(function(mutation) {
            console.log("mutation directive:", mutation.target.textContent)
          });
        });

        var config = {characterData: true, subtree:true};// TODO:
        observer.observe(labelElement[0], config);


        function compileBlock() {

          var labelVars = {
            $contentTypeName: blockObject.content.contentTypeName,
            $settings: blockObject.settingsData || {},
            $layout: blockObject.layout || {},
            $index: blockObject.index + 1,
            ... blockObject.data
          };

          var compileScope = scope.$new(true);
          compileScope = Object.assign(compileScope, labelVars);

          blockObject.renderedLabel = $compile(labelElement.contents())(compileScope);
          console.log('blockObject.renderedLabel:', blockObject.renderedLabel[0])
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
        scope.$watch(function () {
          return blockObject.index;
        }, compileBlock);

        /*
        // No need to watch contentTypeName as it wont change.
        scope.$watch(function () {
          return blockObject.content.contentTypeName;
        }, compileBlock);
        */

        // TODO: Destroy

      }
    };
  });
