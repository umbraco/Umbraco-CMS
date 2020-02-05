
/**
 * @ngdoc controller
 * @name Umbraco.MainController
 * @function
 * 
 * @description
 * The main application controller
 * 
 */
function MainController($scope, $location, appState, treeService, notificationsService, 
    userService, historyService, updateChecker, navigationService, eventsService, 
    tmhDynamicLocale, localStorageService, editorService, overlayService, assetsService, tinyMceAssets) {

    //the null is important because we do an explicit bool check on this in the view
    $scope.authenticated = null;
    $scope.touchDevice = appState.getGlobalState("touchDevice");
    $scope.infiniteMode = false;
    $scope.overlay = {};
    $scope.drawer = {};
    $scope.search = {};
    $scope.login = {};
    $scope.tabbingActive = false;

    // Load TinyMCE assets ahead of time in the background for the user
    // To help with first load of the RTE
    tinyMceAssets.forEach(function (tinyJsAsset) {
        assetsService.loadJs(tinyJsAsset, $scope);
    });

    // There are a number of ways to detect when a focus state should be shown when using the tab key and this seems to be the simplest solution. 
    // For more information about this approach, see https://hackernoon.com/removing-that-ugly-focus-ring-and-keeping-it-too-6c8727fefcd2
    function handleFirstTab(evt) {
        if (evt.keyCode === 9) {
            $scope.tabbingActive = true;
            $scope.$digest();
            window.removeEventListener('keydown', handleFirstTab);
            window.addEventListener('mousedown', disableTabbingActive);
        }
    }
    
    function disableTabbingActive(evt) {
        $scope.tabbingActive = false;
        $scope.$digest();
        window.removeEventListener('mousedown', disableTabbingActive);
        window.addEventListener("keydown", handleFirstTab);
    }

    window.addEventListener("keydown", handleFirstTab);


    $scope.removeNotification = function (index) {
        notificationsService.remove(index);
    };

    $scope.closeSearch = function() {
        appState.setSearchState("show", false);
    };

    $scope.showLoginScreen = function(isTimedOut) {
        $scope.login.isTimedOut = isTimedOut;
        $scope.login.show = true;
    };

    $scope.hideLoginScreen = function() {
        $scope.login.show = false;
    };

    var evts = [];

    //when a user logs out or timesout
    evts.push(eventsService.on("app.notAuthenticated", function (evt, data) {
        $scope.authenticated = null;
        $scope.user = null;
        const isTimedOut = data && data.isTimedOut ? true : false;
        $scope.showLoginScreen(isTimedOut);

        // Remove the localstorage items for tours shown
        // Means that when next logged in they can be re-shown if not already dismissed etc
        localStorageService.remove("emailMarketingTourShown");
        localStorageService.remove("introTourShown");
    }));

    evts.push(eventsService.on("app.userRefresh", function(evt) {
        userService.refreshCurrentUser().then(function(data) {
            $scope.user = data;

            //Load locale file
            if ($scope.user.locale) {
                tmhDynamicLocale.set($scope.user.locale);
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

            var section = appState.getSectionState("currentSection");
            if (section) {
                //if there's a section already assigned, reload it so the tree is cleared
                navigationService.reloadSection(section);
            }

            $location.path("/").search("");
            historyService.removeAll();
            treeService.clearCache();
            editorService.closeAll();
            overlayService.close();

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

    }));

    // events for search
    evts.push(eventsService.on("appState.searchState.changed", function (e, args) {
        if (args.key === "show") {
            $scope.search.show = args.value;
        }
    }));

    // events for drawer
    // manage the help dialog by subscribing to the showHelp appState
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

    // events for overlays
    evts.push(eventsService.on("appState.overlay", function (name, args) {
        $scope.overlay = args;
    }));
    
    // events for tours
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

    // events for backdrop
    evts.push(eventsService.on("appState.backdrop", function (name, args) {
        $scope.backdrop = args;
    }));

    // event for infinite editors
    evts.push(eventsService.on("appState.editors.open", function (name, args) {
        $scope.infiniteMode = args && args.editors.length > 0 ? true : false;
    }));

    evts.push(eventsService.on("appState.editors.close", function (name, args) {
        $scope.infiniteMode = args && args.editors.length > 0 ? true : false;
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
        tmhDynamicLocaleProvider.localeLocationPattern('lib/angular-i18n/angular-locale_{{locale}}.js');
    });
