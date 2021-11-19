(function () {
    "use strict";

    function AppHeaderDirective(eventsService, appState, userService, focusService, backdropService, overlayService) {

        function link(scope, el, attr, ctrl) {

            var evts = [];

            // the null is important because we do an explicit bool check on this in the view
            // the avatar is by default the umbraco logo
            scope.authenticated = null;
            scope.user = null;
            scope.avatar = [
                { value: "assets/img/application/logo.png" },
                { value: "assets/img/application/logo@2x.png" },
                { value: "assets/img/application/logo@3x.png" }
            ];

            // when a user logs out or timesout
            evts.push(eventsService.on("app.notAuthenticated", function () {
                scope.authenticated = false;
                scope.user = null;
            }));

            // when the application is ready and the user is authorized setup the data
            evts.push(eventsService.on("app.ready", function (evt, data) {

                scope.authenticated = true;
                scope.user = data.user;

                if (scope.user.avatars) {
                    scope.avatar = [];
                    if (Utilities.isArray(scope.user.avatars)) {
                        for (var i = 0; i < scope.user.avatars.length; i++) {
                            scope.avatar.push({ value: scope.user.avatars[i] });
                        }
                    }
                }

            }));

            evts.push(eventsService.on("app.userRefresh", function (evt) {
                userService.refreshCurrentUser().then(function (data) {
                    scope.user = data;

                    if (scope.user.avatars) {
                        scope.avatar = [];
                        if (Utilities.isArray(scope.user.avatars)) {
                            for (var i = 0; i < scope.user.avatars.length; i++) {
                                scope.avatar.push({ value: scope.user.avatars[i] });
                            }
                        }
                    }
                });
            }));

            scope.rememberFocus = focusService.rememberFocus;

            scope.searchClick = function () {
                var showSearch = appState.getSearchState("show");
                appState.setSearchState("show", !showSearch);
            };

            // toggle the help dialog by raising the global app state to toggle the help drawer
            scope.helpClick = function () {
                var showDrawer = appState.getDrawerState("showDrawer");
                var drawer = { view: "help", show: !showDrawer };
                appState.setDrawerState("view", drawer.view);
                appState.setDrawerState("showDrawer", drawer.show);
            };

            scope.avatarClick = function () {

                const dialog = {
                    view: "user",
                    position: "right",
                    name: "overlay-user",
                    close: function () {
                        overlayService.close();
                    }
                };

                overlayService.open(dialog);
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
