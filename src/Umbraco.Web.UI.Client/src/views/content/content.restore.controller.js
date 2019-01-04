angular.module("umbraco").controller("Umbraco.Editors.Content.RestoreController",
    function ($scope, relationResource, contentResource, entityResource, navigationService, appState, treeService, localizationService) {

        $scope.source = _.clone($scope.currentNode);

		$scope.error = null;
        $scope.success = false;
        $scope.loading = true;

        relationResource.getByChildId($scope.source.id, "relateParentDocumentOnDelete").then(function (data) {
            $scope.loading = false;

            if (!data.length) {
                localizationService.localizeMany(["recycleBin_itemCannotBeRestored", "recycleBin_noRestoreRelation"])
                    .then(function(values) {
                        $scope.success = false;
                        $scope.error = {
                            errorMsg: values[0],
                            data: {
                                Message: values[1]
                            }
                        }
                    });
                return;
            }

		    $scope.relation = data[0];

			if ($scope.relation.parentId === -1) {
				$scope.target = { id: -1, name: "Root" };

            } else {
			    $scope.loading = true;
                entityResource.getById($scope.relation.parentId, "Document").then(function (data) {
                    $scope.loading = false;
					$scope.target = data;
					// make sure the target item isn't in the recycle bin
					if($scope.target.path.indexOf("-20") !== -1) {
                        localizationService.localizeMany(["recycleBin_itemCannotBeRestored", "recycleBin_restoreUnderRecycled"])
					        .then(function (values) {
					            $scope.success = false;
					            $scope.error = {
					                errorMsg: values[0],
					                data: {
                                        Message: values[1].replace('%0%', $scope.target.name)
					                }
					            }
					        });
						$scope.success = false;
					}

				}, function (err) {
					$scope.success = false;
					$scope.error = err;
                    $scope.loading = false;
				});
			}

		}, function (err) {
			$scope.success = false;
			$scope.error = err;
		});

		$scope.restore = function () {
		    $scope.loading = true;
			// this code was copied from `content.move.controller.js`
            contentResource.move({ parentId: $scope.target.id, id: $scope.source.id })
				.then(function (path) {

                    $scope.loading = false;
					$scope.success = true;

					//first we need to remove the node that launched the dialog
					treeService.removeNode($scope.currentNode);

					//get the currently edited node (if any)
					var activeNode = appState.getTreeState("selectedNode");

					//we need to do a double sync here: first sync to the moved content - but don't activate the node,
					//then sync to the currenlty edited content (note: this might not be the content that was moved!!)

					navigationService.syncTree({ tree: "content", path: path, forceReload: true, activate: false }).then(function (args) {
						if (activeNode) {
							var activeNodePath = treeService.getPath(activeNode).join();
							//sync to this node now - depending on what was copied this might already be synced but might not be
							navigationService.syncTree({ tree: "content", path: activeNodePath, forceReload: false, activate: true });
						}
					});

				}, function (err) {
					$scope.success = false;
					$scope.error = err;
                    $scope.loading = false;
				});
        };

        $scope.close = function () {
            navigationService.hideDialog();
        };

	});
