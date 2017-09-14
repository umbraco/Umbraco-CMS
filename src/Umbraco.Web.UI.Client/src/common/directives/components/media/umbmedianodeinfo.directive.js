(function () {
    'use strict';

    function MediaNodeInfoDirective($timeout, $location) {

        function link(scope, element, attrs, ctrl) {

            function onInit() {

                scope.allowOpenMediaType = true;

                // get document type details
                scope.mediaType = getMediaType(scope.node);

                // get node url
                scope.nodeUrl = getNodeUrl(scope.node);

            }

            scope.openMediaType = function (mediaType) {
                // remove first "#" from url if it is prefixed else the path won't work
                var url = mediaType.url.replace(/^#/, "");
                $location.path(url);
            };

            function getMediaType(node) {

                var mediaType = {};

                // find the document type in the properties array
                angular.forEach(node.properties, function (property) {
                    if (property.alias === "_umb_doctype") {
                        if (property.value && property.value.length > 0) {
                            mediaType = property.value[0];
                        }
                    }
                });

                return mediaType;

            }

            function getNodeUrl(node) {

                var nodeUrl = "";

                // find the document type in the properties array
                angular.forEach(node.properties, function (property) {
                    if (property.alias === "_umb_urls") {
                        if (property.value) {
                            nodeUrl = property.value;
                        }
                    }
                });

                return nodeUrl;

            }

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