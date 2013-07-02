
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
function NavigationController($scope, navigationService, sectionResource) {
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

    sectionResource.getSections()
        .then(function(result) {
            $scope.sections = result;
        }, function (reason) {
            //TODO: handle error properly
            alert(reason);
        });

    //events
    $scope.$on("treeOptionsClick", function (ev, args) {
        $scope.currentNode = args.node;
        args.scope = $scope;
        navigationService.showMenu(ev, args);
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