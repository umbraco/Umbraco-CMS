
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
function NavigationController($scope, $rootScope, $location, $log, $routeParams, appState, navigationService, keyboardService, dialogService, historyService, sectionResource, angularHelper) {

    //TODO: Need to think about this and an nicer way to acheive what this is doing.
    //the tree event handler i used to subscribe to the main tree click events
    $scope.treeEventHandler = $({});
    navigationService.setupTreeEvents($scope.treeEventHandler, $scope);

    //Put the navigation service on this scope so we can use it's methods/properties in the view.
    // IMPORTANT: all properties assigned to this scope are generally available on the scope object on dialogs since
    //   when we create a dialog we pass in this scope to be used for the dialog's scope instead of creating a new one.
    $scope.nav = navigationService;

    //set up our scope vars
    $scope.showContextMenuDialog = false;
    $scope.showSearchResults = false;
    $scope.currentSection = appState.getSectionState("currentSection");
    $scope.showNavigation = appState.getGlobalState("showNavigation");
    
    //trigger search with a hotkey:
    keyboardService.bind("ctrl+shift+s", function(){
        navigationService.showSearch();
    });

    //trigger dialods with a hotkey:
    //TODO: Unfortunately this will also close the login dialog.
    keyboardService.bind("esc", function(){
        $rootScope.$emit("closeDialogs");
    });
    
    $scope.selectedId = navigationService.currentId;
    
    //Listen for global state changes
    $scope.$on("appState.globalState.changed", function (e, args) {
        if (args.key === "showNavigation") {
            $scope.showNavigation = args.value;
        }
    });

    //Listen for menu state changes
    $scope.$on("appState.menuState.changed", function(e, args) {
        if (args.key === "showMenuDialog") {
            $scope.showContextMenuDialog = args.value;
        }
    });
    
    //Listen for section state changes
    $scope.$on("appState.sectionState.changed", function (e, args) {
        //section changed
        if (args.key === "currentSection") {
            $scope.currentSection = args.value;
        }
        //show/hide search results
        if (args.key === "showSearchResults") {
            $scope.showSearchResults = args.value;
        }
    });

    //This reacts to clicks passed to the body element which emits a global call to close all dialogs
    $rootScope.$on("closeDialogs", function (event) {
        if (navigationService.ui.stickyNavigation) {
            navigationService.hideNavigation();
            //TODO: don't know why we need this? - we are inside of an angular event listener.
            angularHelper.safeApply($scope);
        }
    });
        

    //this reacts to the options item in the tree
    //todo, migrate to nav service
    $scope.searchShowMenu = function (ev, args) {   
        $scope.currentNode = args.node;
        args.scope = $scope;

        //always skip default
        args.skipDefault = true;
        navigationService.showMenu(ev, args);
    };

    //todo, migrate to nav service
    $scope.searchHide = function () {   
        navigationService.hideSearch();
    };


    /** Opens a dialog but passes in this scope instance to be used for the dialog */
    $scope.openDialog = function (currentNode, action, currentSection) {        
        navigationService.showDialog({
            scope: $scope,
            node: currentNode,
            action: action,
            section: currentSection
        });
    };
}

//register it
angular.module('umbraco').controller("Umbraco.NavigationController", NavigationController);
