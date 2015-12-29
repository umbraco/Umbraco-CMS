angular.module("umbraco.directives")
   .directive('umbAutoResize', function($timeout) {
      return {
         require: ["^?umbTabs", "ngModel"],
         link: function(scope, element, attr, controllersArr) {

            var domEl = element[0];
            var domElType = domEl.type;
            var umbTabsController = controllersArr[0];
            var ngModelController = controllersArr[1];

            // IE elements
            var isIEFlag = false;
            var wrapper = angular.element('#umb-ie-resize-input-wrapper');
            var mirror = angular.element('<span style="white-space:pre;"></span>');

            function isIE() {

               var ua = window.navigator.userAgent;
               var msie = ua.indexOf("MSIE ");

               if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./) || navigator.userAgent.match(/Edge\/\d+/)) {
                  return true;
               } else {
                  return false;
               }

            }

            function activate() {

               // check if browser is Internet Explorere
               isIEFlag = isIE();

               // scrollWidth on element does not work in IE on inputs
               // we have to do some dirty dom element copying.
               if (isIEFlag === true && domElType === "text") {
                  setupInternetExplorerElements();
               }

            }

            function setupInternetExplorerElements() {

               if (!wrapper.length) {
                  wrapper = angular.element('<div id="umb-ie-resize-input-wrapper" style="position:fixed; top:-999px; left:0;"></div>');
                  angular.element('body').append(wrapper);
               }

               angular.forEach(['fontFamily', 'fontSize', 'fontWeight', 'fontStyle',
                  'letterSpacing', 'textTransform', 'wordSpacing', 'textIndent',
                  'boxSizing', 'borderRightWidth', 'borderLeftWidth', 'borderLeftStyle', 'borderRightStyle',
                  'paddingLeft', 'paddingRight', 'marginLeft', 'marginRight'
               ], function(value) {
                  mirror.css(value, element.css(value));
               });

               wrapper.append(mirror);

            }

            function resizeInternetExplorerInput() {

               mirror.text(element.val() || attr.placeholder);
               element.css('width', mirror.outerWidth() + 1);

            }

            function resizeInput() {

               if (domEl.scrollWidth !== domEl.clientWidth) {
                  if (ngModelController.$modelValue) {
                     element.width(domEl.scrollWidth);
                  }
               }

               if(!ngModelController.$modelValue && attr.placeholder) {
                  attr.$set('size', attr.placeholder.length);
                  element.width('auto');
               }

            }

            function resizeTextarea() {

               if(domEl.scrollHeight !== domEl.clientHeight) {

                  element.height(domEl.scrollHeight);

               }

            }

            var update = function(force) {


               if (force === true) {

                  if (domElType === "textarea") {
                     element.height(0);
                  } else if (domElType === "text") {
                     element.width(0);
                  }

               }


               if (isIEFlag === true && domElType === "text") {

                  resizeInternetExplorerInput();

               } else {

                  if (domElType === "textarea") {

                     resizeTextarea();

                  } else if (domElType === "text") {

                     resizeInput();

                  }

               }

            };

            activate();

            //listen for tab changes
            if (umbTabsController != null) {
               umbTabsController.onTabShown(function(args) {
                  update();
               });
            }

            // listen for ng-model changes
            var unbindModelWatcher = scope.$watch(function() {
               return ngModelController.$modelValue;
            }, function(newValue) {
               update(true);
            });

            scope.$on('$destroy', function() {
               element.unbind('keyup keydown keypress change', update);
               element.unbind('blur', update(true));
               unbindModelWatcher();

               // clean up IE dom element
               if (isIEFlag === true && domElType === "text") {
                  mirror.remove();
               }

            });
         }
      };
   });
