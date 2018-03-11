(function () {
    'use strict';

    function MediaNodeInfoDirective($timeout, $location, eventsService, userService, dateHelper) {

        function link(scope, element, attrs, ctrl) {

            var evts = [];

            function onInit() {
                scope.allowOpenMediaType = true;
                // get document type details
                scope.mediaType = scope.node.contentType;
                // get node url
                scope.nodeUrl = scope.node.mediaLink;
                // make sure dates are formatted to the user's locale
                formatDatesToLocal();
            }

            function formatDatesToLocal() {
                // get current backoffice user and format dates
                userService.getCurrentUser().then(function (currentUser) {
                    scope.node.createDateFormatted = dateHelper.getLocalDate(scope.node.createDate, currentUser.locale, 'LLL');
                    scope.node.updateDateFormatted = dateHelper.getLocalDate(scope.node.updateDate, currentUser.locale, 'LLL');
                });
            }

            scope.openMediaType = function (mediaType) {
                // remove first "#" from url if it is prefixed else the path won't work
                var url = "/settings/mediaTypes/edit/" + mediaType.id;
                $location.path(url);
            };
            
            // watch for content updates - reload content when node is saved, published etc.
            scope.$watch('node.updateDate', function(newValue, oldValue){
                if(!newValue) { return; }
                if(newValue === oldValue) { return; }
                formatDatesToLocal();
            });

            //ensure to unregister from all events!
            scope.$on('$destroy', function () {
                for (var e in evts) {
                    eventsService.unsubscribe(evts[e]);
                }
            });

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