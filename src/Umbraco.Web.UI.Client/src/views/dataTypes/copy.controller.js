angular.module("umbraco")
  .controller("Umbraco.Editors.DataType.CopyController",
    function ($scope, dataTypeResource, treeService, navigationService, appState) {

      $scope.dialogTreeApi = {};
      $scope.source = _.clone($scope.currentNode);

      function nodeSelectHandler(args) {
        args.event.preventDefault();
        args.event.stopPropagation();

        if ($scope.target) {
          //un-select if there's a current one selected
          $scope.target.selected = false;
        }

        $scope.target = args.node;
        $scope.target.selected = true;
      }

      $scope.copy = function () {

        $scope.busy = true;
        $scope.error = false;

        dataTypeResource.copy({ parentId: $scope.target.id, id: $scope.source.id })
          .then(function (path) {
            $scope.error = false;
            $scope.success = true;
            $scope.busy = false;

            //get the currently edited node (if any)
            var activeNode = appState.getTreeState("selectedNode");

            //we need to do a double sync here: first sync to the copied content - but don't activate the node,
            //then sync to the currenlty edited content (note: this might not be the content that was copied!!)

            navigationService.syncTree({ tree: "dataTypes", path: path, forceReload: true, activate: false }).then(function (args) {
              if (activeNode) {
                var activeNodePath = treeService.getPath(activeNode).join();
                //sync to this node now - depending on what was copied this might already be synced but might not be
                navigationService.syncTree({ tree: "dataTypes", path: activeNodePath, forceReload: false, activate: true });
              }
            });

          }, function (err) {
            $scope.success = false;
            $scope.error = err;
            $scope.busy = false;

          });
      };

      $scope.onTreeInit = function () {
        $scope.dialogTreeApi.callbacks.treeNodeSelect(nodeSelectHandler);
      }

      $scope.close = function () {
        navigationService.hideDialog();
      };

    });
