
/**
 * @ngdoc controller
 * @name Umbraco.MainController
 * @function
 * 
 * @description
 * The main application controller
 * 
 */
function MainController($scope, $rootScope, $location, $routeParams, $timeout, $http, $log, appState, treeService, notificationsService, userService, navigationService, historyService, updateChecker, assetsService, eventsService, umbRequestHelper, tmhDynamicLocale, localStorageService, tourService) {

    //the null is important because we do an explicit bool check on this in the view
    //the avatar is by default the umbraco logo    
    $scope.authenticated = null;
    $scope.avatar = [
        { value: "assets/img/application/logo.png" },
        { value: "assets/img/application/logo@2x.png" },
        { value: "assets/img/application/logo@3x.png" }
    ];
    $scope.touchDevice = appState.getGlobalState("touchDevice");


    $scope.removeNotification = function (index) {
        notificationsService.remove(index);
    };

    $scope.closeDialogs = function (event) {
        //only close dialogs if non-link and non-buttons are clicked
        var el = event.target.nodeName;
        var els = ["INPUT", "A", "BUTTON"];

        if (els.indexOf(el) >= 0) { return; }

        var parents = $(event.target).parents("a,button");
        if (parents.length > 0) {
            return;
        }

        //SD: I've updated this so that we don't close the dialog when clicking inside of the dialog
        var nav = $(event.target).parents("#dialog");
        if (nav.length === 1) {
            return;
        }

        eventsService.emit("app.closeDialogs", event);
    };

    var evts = [];

    //when a user logs out or timesout
    evts.push(eventsService.on("app.notAuthenticated", function () {
        $scope.authenticated = null;
        $scope.user = null;
    }));

    evts.push(eventsService.on("app.userRefresh", function(evt) {
        userService.refreshCurrentUser().then(function(data) {
            $scope.user = data;

            //Load locale file
            if ($scope.user.locale) {
                tmhDynamicLocale.set($scope.user.locale);
            }

            if ($scope.user.avatars) {
                $scope.avatar = [];
                if (angular.isArray($scope.user.avatars)) {
                    for (var i = 0; i < $scope.user.avatars.length; i++) {
                        $scope.avatar.push({ value: $scope.user.avatars[i] });
                    }
                }
            }
        });
    }));

    //when the app is ready/user is logged in, setup the data
    evts.push(eventsService.on("app.ready", function (evt, data) {

        $scope.authenticated = data.authenticated;
        $scope.user = data.user;

        updateChecker.check().then(function (update) {
            if (update && update !== "null") {
                if (update.type !== "None") {
                    var notification = {
                        headline: "Update available",
                        message: "Click to download",
                        sticky: true,
                        type: "info",
                        url: update.url
                    };
                    notificationsService.add(notification);
                }
            }
        });

        //if the user has changed we need to redirect to the root so they don't try to continue editing the
        //last item in the URL (NOTE: the user id can equal zero, so we cannot just do !data.lastUserId since that will resolve to true)
        if (data.lastUserId !== undefined && data.lastUserId !== null && data.lastUserId !== data.user.id) {
            $location.path("/").search("");
            historyService.removeAll();
            treeService.clearCache();

            //if the user changed, clearout local storage too - could contain sensitive data
            localStorageService.clearAll();
        }

        //if this is a new login (i.e. the user entered credentials), then clear out local storage - could contain sensitive data
        if (data.loginType === "credentials") {
            localStorageService.clearAll();
        }

        //Load locale file
        if ($scope.user.locale) {
            tmhDynamicLocale.set($scope.user.locale);
        }

        if ($scope.user.avatars) {

            $scope.avatar = [];
            if (angular.isArray($scope.user.avatars)) {
                for (var i = 0; i < $scope.user.avatars.length; i++) {
                    $scope.avatar.push({ value: $scope.user.avatars[i] });
                }
            }

        }
    }));

    evts.push(eventsService.on("app.ysod", function (name, error) {
        $scope.ysodOverlay = {
            view: "ysod",
            error: error,
            show: true
        };
    }));

    // manage the help dialog by subscribing to the showHelp appState
    $scope.drawer = {};
    evts.push(eventsService.on("appState.drawerState.changed", function (e, args) {
        // set view
        if (args.key === "view") {
            $scope.drawer.view = args.value;
        }
        // set custom model
        if (args.key === "model") {
            $scope.drawer.model = args.value;
        }
        // show / hide drawer
        if (args.key === "showDrawer") {
            $scope.drawer.show = args.value;
        }
    }));

    evts.push(eventsService.on("appState.tour.start", function (name, args) {
        $scope.tour = args;
        $scope.tour.show = true;
    }));

    evts.push(eventsService.on("appState.tour.end", function () {
        $scope.tour = null;
    }));

    evts.push(eventsService.on("appState.tour.complete", function () {
        $scope.tour = null;
    }));

    evts.push(eventsService.on("appState.backdrop", function (name, args) {
        $scope.backdrop = args;
    }));

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });

}


//register it
angular.module('umbraco').controller("Umbraco.MainController", MainController).
    config(function (tmhDynamicLocaleProvider) {
        //Set url for locale files
        tmhDynamicLocaleProvider.localeLocationPattern('lib/angular/1.1.5/i18n/angular-locale_{{locale}}.js');
    });
