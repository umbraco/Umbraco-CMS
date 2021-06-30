angular.module("umbraco").controller("Umbraco.Editors.Dictionary.MoveController",
    function ($scope, eventsService, dictionaryResource, navigationService, appState, treeService) {

        $scope.dialogTreeApi = {};
        $scope.busy = false;

        $scope.treeModel = {
            hideHeader: false
        }

        $scope.source = _.clone($scope.currentNode);

        function treeLoadedHandler(args) {
            if ($scope.source && $scope.source.path) {
                $scope.dialogTreeApi.syncTree({ path: $scope.source.path, activate: false });
            }
        }

        function nodeSelectHandler(args) {

            if(args && args.event) {
                args.event.preventDefault();
                args.event.stopPropagation();
            }

            eventsService.emit("editors.dictionary.moveController.select", args);

            if ($scope.target) {
                //un-select if there's a current one selected
                $scope.target.selected = false;
            }

            $scope.target = args.node;
            $scope.target.selected = true;

        }

        $scope.close = function() {
            navigationService.hideDialog();
        };

        $scope.move = function () {

            $scope.busy = true;
            $scope.error = false;

            dictionaryResource.move({ parentId: $scope.target.id, id: $scope.source.id })
                .then(function (path) {
                    $scope.error = false;
                    $scope.success = true;
                    $scope.busy = false;

                    //first we need to remove the node that launched the dialog
                    treeService.removeNode($scope.currentNode);

                    //get the currently edited node (if any)
                    var activeNode = appState.getTreeState("selectedNode");

                    //we need to do a double sync here: first sync to the moved content - but don't activate the node,
                    //then sync to the currently edited content (note: this might not be the content that was moved!!)

                    navigationService.syncTree({ tree: "dictionary", path: path, forceReload: true, activate: false }).then(function (args) {
                        if (activeNode) {
                            var activeNodePath = treeService.getPath(activeNode).join();
                            //sync to this node now - depending on what was copied this might already be synced but might not be
                            navigationService.syncTree({ tree: "dictionary", path: activeNodePath, forceReload: false, activate: true });
                        }
                        eventsService.emit("editors.dictionary.reload", args);
                    });

                }, function (err) {
                    $scope.success = false;
                    $scope.error = err;
                    $scope.busy = false;
                   
                });
        };

        $scope.onTreeInit = function () {
            $scope.dialogTreeApi.callbacks.treeLoaded(treeLoadedHandler);
            $scope.dialogTreeApi.callbacks.treeNodeSelect(nodeSelectHandler);
        }	    
        
        // Mini list view
        $scope.selectListViewNode = function (node) {
            node.selected = node.selected === true ? false : true;
            nodeSelectHandler({ node: node });
        };
    });
