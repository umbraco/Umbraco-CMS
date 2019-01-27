(function () {
    'use strict';

    function MemberGroupNodeInfoDirective(eventsService, userService, dateHelper) {

        function link(scope, element, attrs, ctrl) {

            var evts = [];

            function onInit() {

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

            // watch for content updates - reload content when node is saved, published etc.
            scope.$watch('node.updateDate', function(newValue, oldValue){
                if(!newValue) { return; }
                if(newValue === oldValue) { return; }

                // Update the create and update dates
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
            templateUrl: 'views/components/member/umb-membergroup-node-info.html',
            scope: {
                node: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbMembergroupNodeInfo', MemberGroupNodeInfoDirective);

})();
