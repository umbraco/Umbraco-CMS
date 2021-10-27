angular.module("umbraco.directives")
    .directive('umbOverflowChecker', function ($parse, $timeout, windowResizeListener) {
        return {
          restrict: 'A',
          link: function (scope, element, attrs) {
            const overflow = $parse(attrs.onOverflow);

            const scrollElement = element[0];
            const container = element[0].parentElement;

            function checkOverflow () {
              $timeout(() => {
                const scrollElementScrollWidth = scrollElement.scrollWidth;
                const containerScrollWidth = container.scrollWidth;

                const overflowLeft = scrollElement.scrollLeft;
                const overflowRight = containerScrollWidth - scrollElementScrollWidth + overflowLeft;

                scope.$evalAsync(() => overflow(scope, {overflowLeft, overflowRight}));
              }, 50);
            }

            function scrollTo (event, options) {
              $timeout(() => {
                if (options.position === 'end') {
                  scrollElement.scrollLeft = scrollElement.scrollWidth - scrollElement.clientWidth;
                }

                if (options.position === 'start') {
                  scrollElement.scrollLeft = 0;
                }
              }, 50);
            }

            scrollElement.addEventListener('scroll', checkOverflow);
            windowResizeListener.register(checkOverflow);

            scope.$on('$destroy', () => {
              scrollElement.removeEventListener('scroll', checkOverflow);
              windowResizeListener.unregister(checkOverflow);
            });

            scope.$on('umbOverflowChecker.checkOverflow', checkOverflow);
            scope.$on('umbOverflowChecker.scrollTo', scrollTo);

            checkOverflow();
          }
        }
    });
