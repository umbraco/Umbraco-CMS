(function () {
    "use strict";

    function AppHeaderDirective(eventsService, appState, userService, focusService, $timeout, editorService) {

        function link(scope, element) {

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
            scope.hideBackofficeLogo = Umbraco.Sys.ServerVariables.umbracoSettings.hideBackofficeLogo;

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
                const userEditor = {
                  size: "small",
                  view: "views/common/infiniteeditors/user/user.html",
                  close: function() {
                    editorService.close();
                  }
                };

                editorService.open(userEditor);
            };

            scope.logoModal = {
                show: false,
                text: "",
                timer: null
            };
            scope.showLogoModal = function() {
                $timeout.cancel(scope.logoModal.timer);
                scope.logoModal.show = true;
                scope.logoModal.text = "version "+Umbraco.Sys.ServerVariables.application.version;
                $timeout(function () {
                    const anchorLink = element[0].querySelector('.umb-app-header__logo-modal');
                    if(anchorLink) {
                        anchorLink.focus();
                    }
                });
            };
            scope.keepLogoModal = function() {
                $timeout.cancel(scope.logoModal.timer);
            };
            scope.hideLogoModal = function() {
                if(scope.logoModal.show === true) {
                    $timeout.cancel(scope.logoModal.timer);
                    scope.logoModal.timer = $timeout(function () {
                        scope.logoModal.show = false;
                    }, 100);
                }
            };
            scope.stopClickEvent = function($event) {
                $event.stopPropagation();
            };

            scope.toggleLogoModal = function() {
                if(scope.logoModal.show) {
                    $timeout.cancel(scope.logoModal.timer);
                    scope.logoModal.show = false;
                } else {
                    scope.showLogoModal();
                }
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
