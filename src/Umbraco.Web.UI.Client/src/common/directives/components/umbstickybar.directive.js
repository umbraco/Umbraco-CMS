/**
@ngdoc directive
@name umbraco.directives.directive:umbStickyBar
@restrict A

@description
Use this directive make an element sticky and follow the page when scrolling. `umb-sticky-bar--active` class is applied when the element is stuck

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <div
           class="my-sticky-bar"
           umb-sticky-bar>
        </div>

    </div>
</pre>

**/

(function () {
    'use strict';

    function StickyBarDirective() {

        /**
        On initial load, the intersector fires if the grid editor is in the viewport
        This flag is used to suppress the setClass behaviour on the initial load
        **/
        var initial = true;

        /**
        Toggle `umb-sticky-bar--active` class on the sticky-bar element
        **/
        function setClass(addClass, current) {
            if (!initial) {
                current.classList.toggle('umb-sticky-bar--active', addClass);
            } else {
                initial = false;
            }
        }

        /**
        Inserts two elements in the umbStickyBar parent element
        These are used by the IntersectionObserve to calculate scroll position
        **/
        function addSentinels(current) {
            ['-top', '-bottom'].forEach(s => {
                const sentinel = document.createElement('div');
                sentinel.classList.add('umb-sticky-sentinel', s);
                current.parentElement.appendChild(sentinel);
            });
        }

        /**
        Calls into setClass when the footer sentinel enters/exits the bottom of the container
        Container is the parent element of the umbStickyBar element
        **/
        function observeFooter(current, container) {
            const observer = new IntersectionObserver((records, observer) => {
                let [target, rootBounds, intersected] = [records[0].boundingClientRect, records[0].rootBounds, records[0].intersectionRatio === 1];

                if (target.bottom > rootBounds.top && intersected) {
                    setClass(true, current);
                }
                if (target.top < rootBounds.top && target.bottom < rootBounds.bottom) {
                    setClass(false, current);
                }
            }, {
                threshold: [1],
                root: container
            });

            observer.observe(current.parentElement.querySelector('.umb-sticky-sentinel.-bottom'));
        }

        /**
        Calls into setClass when the header sentinel enters/exits the top of the container
        Container is the parent element of the umbStickyBar element
        **/
        function observeHeader(current, container) {
            const observer = new IntersectionObserver((records, observer) => {
                let [target, rootBounds] = [records[0].boundingClientRect, records[0].rootBounds];

                if (target.bottom < rootBounds.top) {
                    setClass(true, current);
                }

                if (target.bottom >= rootBounds.top && target.bottom < rootBounds.bottom) {
                    setClass(false, current);
                }
            }, {
                threshold: [0],
                root: container
            });

            observer.observe(current.parentElement.querySelector('.umb-sticky-sentinel.-top'));
        }

        function link(scope, el, attr, ctrl) {

            let current = el[0];
            let container = current.closest('[data-element="editor-container"]');

            addSentinels(current);

            observeHeader(current, container);
            observeFooter(current, container);
        }

        var directive = {
            restrict: 'A',
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbStickyBar', StickyBarDirective);

})();
