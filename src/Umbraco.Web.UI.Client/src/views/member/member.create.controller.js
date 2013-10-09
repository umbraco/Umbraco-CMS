/**
 * @ngdoc controller
 * @name Umbraco.Editors.Member.CreateController
 * @function
 * 
 * @description
 * The controller for the member creation dialog
 */
function memberCreateController($scope, $routeParams, memberTypeResource, iconHelper) {

    $scope.currentStep = 1;
    $scope.memberModel = null;
    $scope.memberType = null;

    $scope.submitMember = function () {
        
        //we need to broadcast the saving event for the toggle validators to work
        $scope.$broadcast("saving");
        //ensure the drop down is dirty so the styles validate
        $scope.memberCreate.$setDirty(true);
        
        if ($scope.memberCreate.$invalid) {
            return;
        }

        alert("whoohoo!");

        //#media/media/edit/{{currentNode.id}}?doctype={{docType.alias}}&create=true
    };
    
    $scope.selectMemberType = function (alias) {
        $scope.memberType = alias;
        $scope.currentStep = 2;
    };
    
    

    memberTypeResource.getTypes($scope.currentNode.id).then(function (data) {
        $scope.allowedTypes = iconHelper.formatContentTypeIcons(data);
    });
    
}

angular.module('umbraco').controller("Umbraco.Editors.Member.CreateController", memberCreateController);