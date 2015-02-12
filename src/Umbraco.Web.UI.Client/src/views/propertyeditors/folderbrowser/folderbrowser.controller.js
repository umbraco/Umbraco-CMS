angular.module("umbraco")

.controller("Umbraco.PropertyEditors.FolderBrowserController",
    function ($rootScope, $scope, $routeParams, $timeout, editorState, navigationService) {

        var dialogOptions = $scope.dialogOptions;
        $scope.creating = $routeParams.create;
        $scope.nodeId = $routeParams.id;

        $scope.onUploadComplete = function () {

            //sync the tree - don't force reload since we're not updating this particular node (i.e. its name or anything),
            // then we'll get the resulting tree node which we can then use to reload it's children.
            var path = editorState.current.path;
            navigationService.syncTree({ tree: "media", path: path, forceReload: false }).then(function (syncArgs) {
                navigationService.reloadNode(syncArgs.node);
            });

        }
});
