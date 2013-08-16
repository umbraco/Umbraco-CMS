
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
function NavigationController($scope,$rootScope, $location, $log, navigationService, keyboardService, dialogService, historyService, sectionResource, angularHelper) {

    //Put the navigation service on this scope so we can use it's methods/properties in the view.
    // IMPORTANT: all properties assigned to this scope are generally available on the scope object on dialogs since
    //   when we create a dialog we pass in this scope to be used for the dialog's scope instead of creating a new one.
    $scope.nav = navigationService;

    //trigger search with a hotkey:
    keyboardService.bind("ctrl+shift+s", function(){
        $scope.nav.showTree($scope.ui.currentSection);
    });

    //the tree event handler i used to subscribe to the main tree click events
    $scope.treeEventHandler = $({});

    $scope.selectedId = navigationService.currentId;
    $scope.sections = navigationService.sections;
    
    sectionResource.getSections()
        .then(function(result) {
            $scope.sections = result;
        });

    //This reacts to clicks passed to the body element which emits a global call to close all dialogs
    $rootScope.$on("closeDialogs", function (event) {
        if (navigationService.ui.stickyNavigation && (!event.target || $(event.target).parents(".umb-modalcolumn").size() == 0)) {
           navigationService.hideNavigation();
            angularHelper.safeApply($scope);
        }
    });
        
    //this reacts to the options item in the tree
    $scope.treeEventHandler.bind("treeOptionsClick", function (ev, args) {
        ev.stopPropagation();
        ev.preventDefault();
        
        $scope.currentNode = args.node;
        args.scope = $scope;

        navigationService.showMenu(ev, args);
    });

    //this reacts to tree items themselves being clicked
    //the tree directive should not contain any handling, simply just bubble events
    $scope.treeEventHandler.bind("treeNodeSelect", function (ev, args) {
        ev.stopPropagation();
        ev.preventDefault();
        
        var n = args.node;

        //here we need to check for some legacy tree code
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
            catch(ex) {
                $log.error("Error evaluating js callback from legacy tree node: " + ex);
            }
        }
        else {
            //add action to the history service
            historyService.add({ name: n.name, link: n.routePath, icon: n.icon });
            //not legacy, lets just set the route value and clear the query string if there is one.
            $location.path(n.routePath).search(null);
        }
    });

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
