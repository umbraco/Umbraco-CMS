
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

    evts.push(eventsService.on("appState.startTour", function (name, args) {
        console.log("start tour event", args);
        $scope.tour = { options: args.options, steps: args.steps, show: true };
    }));

    evts.push(eventsService.on("appState.endTour", function (name) {
        console.log("end tour event");
        $scope.tour = { options: null, steps: null, show: false };
    }));

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });


    var testTour = {
        "options": {
            "name": "testTour",
            "alias": "umbTestTour"
        },
        "steps": [
            {
                element: "#applications #section-settings",
                title: "Navigate to the settings sections",
                content: "In the settings section we will find the document types",
                event: "click"
            },
            {
                element: "#tree #node-documentTypes",
                title: "Let's open the context menu",
                content: "Hover the document types node and click the three small dots",
                event: "click"
            },
            {
                element: "#dialog #action-documentType",
                title: "Create document type",
                content: "Click the option to create a document type with a template",
                event: "click"
            },
            {
                element: "#editor-name-field",
                title: "Enter a name for the document type",
                content: "Enter a name for the document type"
            },
            {
                element: "#editor-description",
                title: "Enter a description for the document type",
                content: "Enter a description for the document type"
            },
            {
                element: "[data-element='group-add']",
                title: "Add tab",
                content: "Add new tab",
                event: "click"
            },
            {
                element: "[data-element='group-name']",
                title: "Enter a name",
                content: "Enter a name the tab"
            },
            {
                element: "[data-element='property-add']",
                title: "Add a property",
                content: "Add a property to the tab",
                event: "click"
            },
            {
                element: "[data-element='overlay-property-settings']",
                title: "Property dialog",
                content: "Something something something about the dialog"
            },
            {
                element: "[data-element='overlay-property-settings'] [data-element='property-name']",
                title: "Enter a name",
                content: "Enter a name for the property editor"
            },
            {
                element: "[data-element='overlay-property-settings'] [data-element='property-description']",
                title: "Enter a description",
                content: "Enter a description for the property editor"
            },
            {
                element: "[data-element='overlay-property-settings'] [data-element='editor-add']",
                title: "Add editor",
                content: "Something something something",
                event: "click"
            },
            {
                element: "[data-element='overlay-editor-picker']",
                title: "Editor picker dialog",
                content: "Something something something about the editor picker dialog. This is here you select the type of property bla bla bla."
            },
            {
                element: "[data-element='overlay-editor-picker'] [data-element='editor-Umbraco.Date']",
                title: "Select the Date editor",
                content: "Something something something about the editor settings dialog",
                event: "click"
            },
            {
                element: "[data-element='overlay-editor-settings']",
                title: "Editor settings dialog",
                content: "A loong story about the editor settings dialog...bla bla bla bla bla"
            },
            {
                element: "[data-element='overlay-submit']",
                title: "Submit the editor settings dialog",
                content: "Click submit to save your changes",
                event: "click"
            },
            {
                element: "[data-element='overlay-submit']",
                title: "Submit the property settings dialog",
                content: "Click submit to save your changes",
                event: "click"
            },
            {
                element: "[data-element='overlay-submit']",
                title: "Submit the property settings dialog",
                content: "Click submit to save your changes",
                event: "click"
            }
        ]
    };

    //tourService.startTour(testTour);

}


//register it
angular.module('umbraco').controller("Umbraco.MainController", MainController).
    config(function (tmhDynamicLocaleProvider) {
        //Set url for locale files
        tmhDynamicLocaleProvider.localeLocationPattern('lib/angular/1.1.5/i18n/angular-locale_{{locale}}.js');
    });
