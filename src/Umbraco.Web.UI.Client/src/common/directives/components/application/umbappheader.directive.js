(function () {
    "use strict";

    function AppHeaderDirective(eventsService, appState) {

        function link(scope, el, attr, ctrl) {

            var evts = [];

            // the null is important because we do an explicit bool check on this in the view
            // the avatar is by default the umbraco logo
            scope.authenticated = null;
            scope.avatar = [
                { value: "assets/img/application/logo.png" },
                { value: "assets/img/application/logo@2x.png" },
                { value: "assets/img/application/logo@3x.png" }
            ];

            // when a user logs out or timesout
            evts.push(eventsService.on("app.notAuthenticated", function() {
                scope.authenticated = false;
            }));

            // when the application is ready and the user is authorized setup the data
            evts.push(eventsService.on("app.ready", function(evt, data) {
                scope.authenticated = true;
            }));

            // toggle the help dialog by raising the global app state to toggle the help drawer
            scope.helpClick = function () {
                var showDrawer = appState.getDrawerState("showDrawer");
                var drawer = { view: "help", show: !showDrawer };
                appState.setDrawerState("view", drawer.view);
                appState.setDrawerState("showDrawer", drawer.show);
            };

            scope.avatarClick = function () {
                scope.userDialog = {
                    view: "user",
                    show: true,
                    close: function (oldModel) {
                        scope.userDialog.show = false;
                        scope.userDialog = null;
                    }
                };
            };

        }

        var directive = {
            transclude: true,
            restrict: "E",
            replace: true,
            templateUrl: "views/components/application/umb-app-header.html",
            link: link,
            scope: {}
        };

        return directive;

    }

    angular.module("umbraco.directives").directive("umbAppHeader", AppHeaderDirective);

})();