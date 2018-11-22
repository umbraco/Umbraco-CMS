/**
 * @ngdoc controller
 * @name Umbraco.Editors.Member.CreateController
 * @function
 * 
 * @description
 * The controller for the member creation dialog
 */
function memberCreateController($scope, memberTypeResource, iconHelper, navigationService) {
    
    memberTypeResource.getTypes($scope.currentNode.id).then(function (data) {
        $scope.allowedTypes = iconHelper.formatContentTypeIcons(data);
    });

    $scope.close = function() {
        const showMenu = true;
        navigationService.hideDialog(showMenu);
    };
    
}

angular.module('umbraco').controller("Umbraco.Editors.Member.CreateController", memberCreateController);