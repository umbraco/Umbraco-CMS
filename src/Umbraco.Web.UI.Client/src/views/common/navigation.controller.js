
/**
 * @ngdoc controller
 * @name NavigationController
 * @function
 * 
 * @description
 * Handles the section area of the app
 * 
 * @param {navigationService} navigationService A reference to the navigationService
 */
function NavigationController($scope,$rootScope, $location, $log, navigationService, dialogService, historyService, sectionResource, angularHelper) {

    //Put the navigation service on this scope so we can use it's methods/properties in the view.
    // IMPORTANT: all properties assigned to this scope are generally available on the scope object on dialogs since
    //   when we create a dialog we pass in this scope to be used for the dialog's scope instead of creating a new one.
    $scope.nav = navigationService;

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
    $scope.$on("treeOptionsClick", function (ev, args) {        
        $scope.currentNode = args.node;
        args.scope = $scope;
        navigationService.showMenu(ev, args);
    });

    //this reacts to tree items themselves being clicked
    //the tree directive should not contain any handling, simply just bubble events
    $scope.$on("treeNodeSelect", function (ev, args) {
        
        var n = args.node;

        //here we need to check for some legacy tree code
        if (n.jsClickCallback && n.jsClickCallback !== "") {
            //this is a legacy tree node!                
            var jsPrefix = "javascript:";
            var js;
            if (n.jsClickCallback.startsWith(jsPrefix)) {
                js = n.jsClickCallback.substr(jsPrefix.length);
            }
            else {
                js = n.jsClickCallback;
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
            historyService.add({name: n.name, link: n.view, icon: n.icon});
            //not legacy, lets just set the route value and clear the query string if there is one.
            $location.path(n.view).search(null);
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
angular.module('umbraco').controller("NavigationController", NavigationController);
