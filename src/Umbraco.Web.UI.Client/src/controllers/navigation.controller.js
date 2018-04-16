
/**
 * @ngdoc controller
 * @name Umbraco.NavigationController
 * @function
 *
 * @description
 * Handles the section area of the app
 *
 * @param {navigationService} navigationService A reference to the navigationService
 */
function NavigationController($scope, $rootScope, $location, $log, $routeParams, $timeout, appState, navigationService, keyboardService, dialogService, historyService, eventsService, sectionResource, angularHelper, languageResource) {

    $scope.treeApi = {};

    //Bind to the main tree events
    $scope.onTreeInit = function () {
        //when a tree is loaded into a section, we need to put it into appState
        $scope.treeApi.callbacks.treeLoaded(function (args) {
            appState.setTreeState("currentRootNode", args.tree);
        });

        //when a tree node is synced this event will fire, this allows us to set the currentNode
        $scope.treeApi.callbacks.treeSynced(function (args) {

            if (args.activate === undefined || args.activate === true) {
                //set the current selected node
                appState.setTreeState("selectedNode", args.node);
                //when a node is activated, this is the same as clicking it and we need to set the
                //current menu item to be this node as well.
                appState.setMenuState("currentNode", args.node);
            }
        });

        //this reacts to the options item in the tree
        $scope.treeApi.callbacks.treeOptionsClick(function (args) {
            args.event.stopPropagation();
            args.event.preventDefault();

            //Set the current action node (this is not the same as the current selected node!)
            appState.setMenuState("currentNode", args.node);

            if (args.event && args.event.altKey) {
                args.skipDefault = true;
            }

            navigationService.showMenu(args);
        });

        $scope.treeApi.callbacks.treeNodeAltSelect(function (args) {
            args.event.stopPropagation();
            args.event.preventDefault();

            args.skipDefault = true;
            navigationService.showMenu(args);
        });

        //this reacts to tree items themselves being clicked
        //the tree directive should not contain any handling, simply just bubble events
        $scope.treeApi.callbacks.treeNodeSelect(function (args) {
            var n = args.node;
            args.event.stopPropagation();
            args.event.preventDefault();

            if (n.metaData && n.metaData["jsClickCallback"] && angular.isString(n.metaData["jsClickCallback"]) && n.metaData["jsClickCallback"] !== "") {
                //this is a legacy tree node!
                var jsPrefix = "javascript:";
                var js;
                if (n.metaData["jsClickCallback"].startsWith(jsPrefix)) {
                    js = n.metaData["jsClickCallback"].substr(jsPrefix.length);
                }
                else {
                    js = n.metaData["jsClickCallback"];
                }
                try {
                    var func = eval(js);
                    //this is normally not necessary since the eval above should execute the method and will return nothing.
                    if (func != null && (typeof func === "function")) {
                        func.call();
                    }
                }
                catch (ex) {
                    $log.error("Error evaluating js callback from legacy tree node: " + ex);
                }
            }
            else if (n.routePath) {
                //add action to the history service
                historyService.add({ name: n.name, link: n.routePath, icon: n.icon });

                //put this node into the tree state
                appState.setTreeState("selectedNode", args.node);
                //when a node is clicked we also need to set the active menu node to this node
                appState.setMenuState("currentNode", args.node);

                //not legacy, lets just set the route value and clear the query string if there is one.
                $location.path(n.routePath).search("");
            }
            else if (args.element.section) {
                $location.path(args.element.section).search("");
            }

            navigationService.hideNavigation();
        });

        //once this is wired up, the nav is ready
        eventsService.emit("app.navigationReady", { treeApi: $scope.treeApi});
    }

    //Put the navigation service on this scope so we can use it's methods/properties in the view.
    // IMPORTANT: all properties assigned to this scope are generally available on the scope object on dialogs since
    //   when we create a dialog we pass in this scope to be used for the dialog's scope instead of creating a new one.
    $scope.nav = navigationService;
    // TODO: Lets fix this, it is less than ideal to be passing in the navigationController scope to something else to be used as it's scope,
    // this is going to lead to problems/confusion. I really don't think passing scope's around is very good practice.
    $rootScope.nav = navigationService;

    //set up our scope vars
    $scope.showContextMenuDialog = false;
    $scope.showContextMenu = false;
    $scope.showSearchResults = false;
    $scope.menuDialogTitle = null;
    $scope.menuActions = [];
    $scope.menuNode = null;
    $scope.languages = [];
    $scope.selectedLanguage = {};
    $scope.page = {};
    $scope.page.languageSelectorIsOpen = false;

    $scope.currentSection = appState.getSectionState("currentSection");
    $scope.showNavigation = appState.getGlobalState("showNavigation");

    //trigger search with a hotkey:
    keyboardService.bind("ctrl+shift+s", function () {
        navigationService.showSearch();
    });

    //trigger dialods with a hotkey:
    keyboardService.bind("esc", function () {
        eventsService.emit("app.closeDialogs");
    });

    $scope.selectedId = navigationService.currentId;

    var evts = [];

    //Listen for global state changes
    evts.push(eventsService.on("appState.globalState.changed", function(e, args) {
        if (args.key === "showNavigation") {
            $scope.showNavigation = args.value;
        }
    }));

    //Listen for menu state changes
    evts.push(eventsService.on("appState.menuState.changed", function(e, args) {
        if (args.key === "showMenuDialog") {
            $scope.showContextMenuDialog = args.value;
        }
        if (args.key === "showMenu") {
            $scope.showContextMenu = args.value;
        }
        if (args.key === "dialogTitle") {
            $scope.menuDialogTitle = args.value;
        }
        if (args.key === "menuActions") {
            $scope.menuActions = args.value;
        }
        if (args.key === "currentNode") {
            $scope.menuNode = args.value;
        }
    }));

    //Listen for section state changes
    evts.push(eventsService.on("appState.treeState.changed", function(e, args) {
        var f = args;
        if (args.value.root && args.value.root.metaData.containsTrees === false) {
            $rootScope.emptySection = true;
        }
        else {
            $rootScope.emptySection = false;
        }
    }));

    //Listen for section state changes
    evts.push(eventsService.on("appState.sectionState.changed", function(e, args) {
        //section changed
        if (args.key === "currentSection") {
            $scope.currentSection = args.value;
        }
        //show/hide search results
        if (args.key === "showSearchResults") {
            $scope.showSearchResults = args.value;
        }
    }));

    // Listen for language updates
    evts.push(eventsService.on("editors.languages.languageDeleted", function(e, args) {
        languageResource.getAll().then(function(languages) {
            $scope.languages = languages;
        });
    }));

    evts.push(eventsService.on("editors.languages.languageCreated", function(e, args) {
        languageResource.getAll().then(function(languages) {
            $scope.languages = languages;
        });
    }));

    //This reacts to clicks passed to the body element which emits a global call to close all dialogs
    evts.push(eventsService.on("app.closeDialogs", function(event) {
        if (appState.getGlobalState("stickyNavigation")) {
            navigationService.hideNavigation();
            //TODO: don't know why we need this? - we are inside of an angular event listener.
            angularHelper.safeApply($scope);
        }
    }));

    //when a user logs out or timesout
    evts.push(eventsService.on("app.notAuthenticated", function() {
        $scope.authenticated = false;
    }));

    //when the application is ready and the user is authorized setup the data
    evts.push(eventsService.on("app.ready", function(evt, data) {
        $scope.authenticated = true;
        
        // load languages
        languageResource.getAll().then(function(languages) {
            $scope.languages = languages;

            // select the default language
            $scope.languages.forEach(function(language) {
                if(language.isDefault) {
                    $scope.selectLanguage(language);
                }
            });

        });

    }));

    $scope.selectLanguage = function(language, languages) {
        $scope.selectedLanguage = language;
        // close the language selector
        $scope.page.languageSelectorIsOpen = false;
    };

    //this reacts to the options item in the tree
    //TODO: migrate to nav service
    //TODO: is this used? 
    $scope.searchShowMenu = function (ev, args) {
        //always skip default
        args.skipDefault = true;
        navigationService.showMenu(args);
    };

    //TODO: migrate to nav service
    //TODO: is this used?
    $scope.searchHide = function () {
        navigationService.hideSearch();
    };

    //the below assists with hiding/showing the tree
    var treeActive = false;

    //Sets a service variable as soon as the user hovers the navigation with the mouse
    //used by the leaveTree method to delay hiding
    $scope.enterTree = function (event) {
        treeActive = true;
    };

    // Hides navigation tree, with a short delay, is cancelled if the user moves the mouse over the tree again
    $scope.leaveTree = function(event) {
        //this is a hack to handle IE touch events which freaks out due to no mouse events so the tree instantly shuts down
        if (!event) {
            return;
        }
        if (!appState.getGlobalState("touchDevice")) {
            treeActive = false;
            $timeout(function() {
                if (!treeActive) {
                    navigationService.hideTree();
                }
            }, 300);
        }
    };
    
    $scope.toggleLanguageSelector = function() {
        $scope.page.languageSelectorIsOpen = !$scope.page.languageSelectorIsOpen;
    };

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }
    });
}

//register it
angular.module('umbraco').controller("Umbraco.NavigationController", NavigationController);
