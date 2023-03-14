/**
@ngdoc directive
@name umbraco.directives.directive:umbImageLazyLoad
@restrict E
@scope

@description
Use this directive to lazy-load an image only when it is scrolled into view.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">
        <img umb-image-lazy-load="{{vm.imageUrl}}" />
    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller() {

            var vm = this;
            vm.imageUrl = "/media/TODO";
        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

**/

(function () {
    'use strict';

    function ImageLazyLoadDirective() {

      const placeholder = "assets/img/transparent.png";

      function link(scope, element, attrs) {

            const observer = new IntersectionObserver(loadImg);
            const img = element[0];
            img.src = placeholder;
            img.classList.add("lazy");
            observer.observe(img);

            function loadImg(entries) {
                entries.forEach(entry => {
                  if (entry.intersectionRatio > 0 && entry.target.src.indexOf(placeholder) > 0) {
                      let lazyImage = entry.target;
                      lazyImage.src = attrs.umbImageLazyLoad;
                      lazyImage.classList.add("loaded");
                      observer.unobserve(lazyImage);
                    }
                });
            }

            // make sure to disconnect the observer when the scope is destroyed
            scope.$on('$destroy', function () {
                observer.disconnect();
            });
        }

        var directive = {
            restrict: "A",
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbImageLazyLoad', ImageLazyLoadDirective);

})();
