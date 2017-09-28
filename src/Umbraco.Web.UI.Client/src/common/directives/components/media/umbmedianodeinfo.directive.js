(function () {
    'use strict';

    function MediaNodeInfoDirective($timeout, $location) {

        function link(scope, element, attrs, ctrl) {

            function onInit() {

                scope.allowOpenMediaType = true;

                // get document type details
                scope.mediaType = scope.node.documentType.value;

                // get node url
                scope.nodeUrl = scope.node.mediaLink;

            }

            scope.openMediaType = function (mediaType) {
                // remove first "#" from url if it is prefixed else the path won't work
                var url = "/settings/mediaTypes/edit/" + mediaType.id;
                $location.path(url);
            };

            onInit();

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/media/umb-media-node-info.html',
            scope: {
                node: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbMediaNodeInfo', MediaNodeInfoDirective);

})();