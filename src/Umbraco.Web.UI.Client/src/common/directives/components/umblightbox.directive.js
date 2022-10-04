/**
@ngdoc directive
@name umbraco.directives.directive:umbLightbox
@restrict E
@scope

@description
<p>Use this directive to open a gallery in a lightbox overlay.</p>

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <div class="my-gallery">
            <button type="button" ng-repeat="image in images" ng-click="vm.openLightbox($index, images)">
                <img ng-src="image.source" />
            </button>
        </div>

        <umb-lightbox
            ng-if="vm.lightbox.show"
            items="vm.lightbox.items"
            active-item-index="vm.lightbox.activeIndex"
            on-close="vm.closeLightbox">
        </umb-lightbox>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {

        "use strict";

        function Controller() {

            var vm = this;

            vm.images = [
                {
                    "source": "linkToImage"
                },
                {
                    "source": "linkToImage"
                }
            ]

            vm.openLightbox = openLightbox;
            vm.closeLightbox = closeLightbox;

            function openLightbox(itemIndex, items) {
                vm.lightbox = {
                    show: true,
                    items: items,
                    activeIndex: itemIndex
                };
            }

            function closeLightbox() {
                vm.lightbox.show = false;
                vm.lightbox = null;
            }

        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

@param {array} items Array of gallery items.
@param {callback} onClose Callback when the lightbox is closed.
@param {number} activeItemIndex Index of active item.
**/


(function() {
    'use strict';

    function LightboxDirective(focusLockService) {

        function link(scope, el, attr, ctrl) {


            function activate() {

                var eventBindings = [];

                el.appendTo("body");

                
                focusLockService.addInertAttribute();

                // clean up
                scope.$on('$destroy', function() {
                    // unbind watchers
                    for (var e in eventBindings) {
                        eventBindings[e]();
                    }
                    
                    focusLockService.removeInertAttribute();

                    document.getElementsByClassName("umb-lightbox__close")[0].blur();
                    el.remove();
                });
            }

            scope.next = function() {

                var nextItemIndex = scope.activeItemIndex + 1;

                if( nextItemIndex < scope.items.length) {
                    scope.items[scope.activeItemIndex].active = false;
                    scope.items[nextItemIndex].active = true;
                    scope.activeItemIndex = nextItemIndex;
                }
            };

            scope.prev = function() {

                var prevItemIndex = scope.activeItemIndex - 1;

                if( prevItemIndex >= 0) {
                    scope.items[scope.activeItemIndex].active = false;
                    scope.items[prevItemIndex].active = true;
                    scope.activeItemIndex = prevItemIndex;
                }

            };

            scope.close = function() {
                if(scope.onClose) {
                    scope.onClose();
                }
            };

            activate();

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-lightbox.html',
            scope: {
                items: '=',
                onClose: "=",
                activeItemIndex: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbLightbox', LightboxDirective);

})();
