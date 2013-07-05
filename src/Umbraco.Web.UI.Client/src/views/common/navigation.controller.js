
/**
 * @ngdoc controller
 * @name NavigationController
 * @function
 * 
 * @description
 * Handles the section area of the app
 * 
 * @param navigationService {navigationService} A reference to the navigationService
 */
function NavigationController($scope, $location, navigationService, sectionResource) {
    //load navigation service handlers
    $scope.changeSection = navigationService.changeSection;
    $scope.showTree = navigationService.showTree;
    $scope.hideTree = navigationService.hideTree;
    $scope.hideMenu = navigationService.hideMenu;
    $scope.showMenu = navigationService.showMenu;
    $scope.hideDialog = navigationService.hideDialog;
    $scope.hideNavigation = navigationService.hideNavigation;
    $scope.ui = navigationService.ui;

    $scope.selectedId = navigationService.currentId;
    $scope.sections = navigationService.sections;

    sectionResource.getSections()
        .then(function(result) {
            $scope.sections = result;
        }, function (reason) {
            //TODO: handle error properly
            alert(reason);
        });

    //events
    
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
            //not legacy, lets just set the route value
            $location.path(n.view);
        }
    });

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
