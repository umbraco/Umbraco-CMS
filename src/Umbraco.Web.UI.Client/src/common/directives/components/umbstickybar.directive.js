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

        let headerObserver;

        /**
        Toggle `umb-sticky-bar--active` class on the sticky-bar element
        **/
        const setClass = (addClass, current) => current.classList.toggle('umb-sticky-bar--active', addClass);                    

        /**
        Inserts two elements in the umbStickyBar parent element
        These are used by the IntersectionObserve to calculate scroll position
        **/
        const addSentinel = current => {
            const sentinel = document.createElement('div');
            sentinel.classList.add('umb-sticky-sentinel', '-top');
            current.parentElement.prepend(sentinel);
        };

        /**
        Calls into setClass when the header sentinel enters/exits the top of the container
        Container is the parent element of the umbStickyBar element
        **/
        const observeHeader = (current, container) => {
            headerObserver = new IntersectionObserver((records, observer) => {
                let [target, rootBounds] = [records[0].boundingClientRect, records[0].rootBounds];

                if (rootBounds && target) {
                    if (target.bottom < rootBounds.top) {
                        setClass(true, current);
                    }

                    if (target.bottom >= rootBounds.top && target.bottom < rootBounds.bottom) {
                        setClass(false, current);
                    }
                }
            }, {
                threshold: [0],
                root: container
            });

            headerObserver.observe(current.parentElement.querySelector('.umb-sticky-sentinel.-top'));
        };

        function link(scope, el, attr, ctrl) {

            let current = el[0];
            let container = current.closest('.umb-editor-container') || current.closest('.umb-dashboard');

            if (container) {
                addSentinel(current);
                observeHeader(current, container);
            }

            scope.$on('$destroy', () => {
                headerObserver.disconnect();
            });
        }

        const directive = {
            restrict: 'A',
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbStickyBar', StickyBarDirective);

})();
