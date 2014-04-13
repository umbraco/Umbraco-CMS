/**
 * @ngdoc directive
 * @name umbraco.directives.directive:resizeToContent
 * @element div
 * @function
 *
 * @description
 * Resize iframe's automatically to fit to the content they contain
 *
 * @example
   <example module="umbraco.directives">
     <file name="index.html">
         <iframe resize-to-content src="meh.html"></iframe>
     </file>
   </example>
 */
angular.module("umbraco.directives")
  .directive('resizeToContent', function ($window, $timeout) {
    return function (scope, el, attrs) {
       var iframe = el[0];
       var iframeWin = iframe.contentWindow || iframe.contentDocument.parentWindow;
       if (iframeWin.document.body) {

          $timeout(function(){
              var height = iframeWin.document.documentElement.scrollHeight || iframeWin.document.body.scrollHeight;
              el.height(height);
          }, 3000);
       }
    };
  });
