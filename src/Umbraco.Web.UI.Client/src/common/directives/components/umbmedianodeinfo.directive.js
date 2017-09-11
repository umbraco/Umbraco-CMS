(function () {
    'use strict';

    function MediaNodeInfoDirective($timeout, $location) {

        function link(scope, element, attrs, ctrl) {

            function onInit() {

                scope.allowOpen = true;

                // get document type details
                scope.documentType = getDocumentType(scope.node);
                scope.node.createDate = "2017-08-28 15:12:30";

            }

            scope.openDocumentType = function (documentType) {
                // remove first "#" from url if it is prefixed else the path won't work
                var url = documentType.url.replace(/^#/, "");
                $location.path(url);
            };
            
/*             function getBla(node) {
                
                var bla = {};

                // finding the create date in properties array
                angular.forEach(node.properties, function (property){
                    if (property.alias === "_umb_createdate") {
                        if (property.value && property.value.length > 0) {
                            bla = property.value[0];
                        }
                    }
                });

                return bla;
            } */

            onInit();
        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-media-node-info.html'
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbMediaNodeInfo', MediaNodeInfoDirective);

})();