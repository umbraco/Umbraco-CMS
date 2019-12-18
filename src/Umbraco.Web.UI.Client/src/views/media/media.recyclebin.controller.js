/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.MediaRecycleBinController
 * @function
 * 
 * @description
 * Controls the recycle bin for media
 * 
 */

function MediaRecycleBinController($scope, $routeParams, mediaResource, navigationService, localizationService) {

    //ensures the list view doesn't actually load until we query for the list view config
    // for the section
    $scope.page = {};
    $scope.page.name = "Recycle Bin";
    $scope.page.nameLocked = true;

    //ensures the list view doesn't actually load until we query for the list view config
    // for the section
    $scope.listViewPath = null;

    $routeParams.id = "-21";
    mediaResource.getRecycleBin().then(function (result) {
        $scope.content = result;
    });

    // sync tree node
    navigationService.syncTree({ tree: "media", path: ["-1", $routeParams.id], forceReload: false });

    localizePageName();

    function localizePageName() {

        var pageName = "general_recycleBin";

        localizationService.localize(pageName).then(function (value) {
            $scope.page.name = value;
        });

    }
}

angular.module('umbraco').controller("Umbraco.Editors.Media.RecycleBinController", MediaRecycleBinController);
