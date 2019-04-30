/**
@ngdoc directive
@name umbraco.directives.directive:umbStickyBar
@restrict A

@description
Use this directive make an element sticky and follow the page when scrolling.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <div
           class="my-sticky-bar"
           umb-sticky-bar
           scrollable-container=".container">
        </div>

    </div>
</pre>

<h3>CSS example</h3>
<pre>
    .my-sticky-bar {
        padding: 15px 0;
        background: #000000;
        position: relative;
        top: 0;
    }

    .my-sticky-bar.-umb-sticky-bar {
        top: 100px;
    }
</pre>

@param {string} scrollableContainer Set the class (".element") or the id ("#element") of the scrollable container element.
**/

(function () {
    'use strict';

    function StickyBarDirective($rootScope) {

        function link(scope, el, attr, ctrl) {

            var bar = $(el);
            var scrollableContainer = null;
            var clonedBar = null;
            var cloneIsMade = false;

            function activate() {

                if (bar.parents(".umb-property").length > 1) {
                    bar.addClass("nested");
                    return;
                }

                if (attr.scrollableContainer) {
                    scrollableContainer = bar.closest(attr.scrollableContainer);
                } else {
                    scrollableContainer = $(window);
                }

                scrollableContainer.on('scroll.umbStickyBar', determineVisibility).trigger("scroll");
                $(window).on('resize.umbStickyBar', determineVisibility);

                scope.$on('$destroy', function () {
                    scrollableContainer.off('.umbStickyBar');
                    $(window).off('.umbStickyBar');
                });

            }

            function determineVisibility() {

                var barTop = bar[0].offsetTop;
                var scrollTop = scrollableContainer.scrollTop();

                if (scrollTop > barTop) {

                    if (!cloneIsMade) {

                        createClone();

                        clonedBar.css({
                            'visibility': 'visible'
                        });

                    } else {

                        calculateSize();

                    }

                } else {

                    if (cloneIsMade) {

                        //remove cloned element (switched places with original on creation)
                        bar.remove();
                        bar = clonedBar;
                        clonedBar = null;

                        bar.removeClass('-umb-sticky-bar');
                        bar.css({
                            position: 'relative',
                            'width': 'auto',
                            'height': 'auto',
                            'z-index': 'auto',
                            'visibility': 'visible'
                        });

                        cloneIsMade = false;

                    }

                }

            }

            function calculateSize() {
                var width = bar.innerWidth();
                clonedBar.css({
                    width: width + 10 // + 10 (5*2) because we need to add border to avoid seeing the shadow beneath. Look at the CSS.
                });
            }

            function createClone() {
                //switch place with cloned element, to keep binding intact
                clonedBar = bar;
                bar = clonedBar.clone();
                clonedBar.after(bar);
                clonedBar.addClass('-umb-sticky-bar');
                clonedBar.css({
                    'position': 'fixed',
                    // if you change this z-index value, make sure the sticky editor sub headers do not 
                    // clash with umb-dropdown (e.g. the content actions dropdown in content list view)
                    'z-index': 99, 
                    'visibility': 'hidden'
                });

                cloneIsMade = true;
                calculateSize();

            }

            activate();

        }

        var directive = {
            restrict: 'A',
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbStickyBar', StickyBarDirective);

})();
