(function () {
    'use strict';

    function MemberNodeInfoDirective($timeout, $location, eventsService, userService, dateHelper, editorService) {

        function link(scope, element, attrs, ctrl) {

            var evts = [];

            //TODO: Infinite editing is not working yet.
            scope.allowChangeMemberType = false;

            function onInit() {
                
                userService.getCurrentUser().then(function (user) {
                    // only allow change of member type if user has access to the settings sections
                    Utilities.forEach(user.sections, function (section) {
                        if (section.alias === "settings") {
                            scope.allowChangeMemberType = true;
                        }
                    });
                });

                // get member type details
                scope.memberType = scope.node.contentType;

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

            scope.openMemberType = function (memberType) {
                var editor = {
                    id: memberType.id,
                    submit: function (model) {
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.memberTypeEditor(editor);
            };

            // watch for content updates - reload content when node is saved, published etc.
            scope.$watch('node.updateDate', function (newValue, oldValue) {
                if (!newValue) { return; }
                if (newValue === oldValue) { return; }

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
            templateUrl: 'views/components/member/umb-member-node-info.html',
            scope: {
                node: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbMemberNodeInfo', MemberNodeInfoDirective);

})();
