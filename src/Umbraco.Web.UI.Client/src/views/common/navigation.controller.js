
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
function NavigationController($scope,$rootScope, $location, $log, $routeParams, navigationService, keyboardService, dialogService, historyService, sectionResource, angularHelper) {

    //Put the navigation service on this scope so we can use it's methods/properties in the view.
    // IMPORTANT: all properties assigned to this scope are generally available on the scope object on dialogs since
    //   when we create a dialog we pass in this scope to be used for the dialog's scope instead of creating a new one.
    $scope.nav = navigationService;

    //wire up the screensize and tree mode detection
    $scope.nav.init();

    //the tree event handler i used to subscribe to the main tree click events
    $scope.treeEventHandler = $({});
    $scope.nav.setupTreeEvents($scope.treeEventHandler, $scope);

    //keep track of 
    $scope.$watch(function () {
        //watch the route parameters section
        return $routeParams.section;
    }, function(newVal, oldVal) {
            $scope.nav.ui.currentSection = newVal;    
    });


    //trigger search with a hotkey:
    keyboardService.bind("ctrl+shift+s", function(){
        $scope.nav.showSearch();
    });

    //trigger dialogs closing with a hotkey:
    keyboardService.bind("esc", function(){
        $rootScope.$emit("closeDialogs");
    });


    $scope.selectedId = navigationService.currentId;
    
    //This reacts to clicks passed to the body element which emits a global call to close all dialogs
    $rootScope.$on("closeDialogs", function (event) {
        if (navigationService.ui.stickyNavigation) {
            navigationService.hideNavigation();
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
