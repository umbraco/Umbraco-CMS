//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Dialogs.LinkPickerController",
	function ($scope, eventsService, dialogService, entityResource, contentResource, mediaHelper, userService) {
	    var dialogOptions = $scope.dialogOptions;

	    $scope.dialogTreeEventHandler = $({});
	    $scope.target = {};

	    if (dialogOptions.currentTarget) {
	        $scope.target = dialogOptions.currentTarget;

	        //if we have a node ID, we fetch the current node to build the form data
	        if ($scope.target.id) {

	            if (!$scope.target.path) {
	                entityResource.getPath($scope.target.id, "Document").then(function (path) {
	                    $scope.target.path = path;
	                    //now sync the tree to this path
	                    $scope.dialogTreeEventHandler.syncTree({ path: $scope.target.path, tree: "content" });
	                });
	            }

	            contentResource.getNiceUrl($scope.target.id).then(function (url) {
	                $scope.target.url = url;
	            });
	        }
	    }

	    $scope.switchToMediaPicker = function () {
	        userService.getCurrentUser().then(function (userData) {
	            dialogService.mediaPicker({
	                startNodeId: userData.startMediaId,
	                callback: function (media) {
	                    $scope.target.id = media.id;
	                    $scope.target.isMedia = true;
	                    $scope.target.name = media.name;
	                    $scope.target.url = mediaHelper.resolveFile(media);
	                }
	            });
	        });
	    };

	    function nodeSelectHandler (ev, args) {
	        args.event.preventDefault();
	        args.event.stopPropagation();

	        eventsService.emit("dialogs.linkPicker.select", args);
            
	        if ($scope.currentNode) {
                //un-select if there's a current one selected
	            $scope.currentNode.selected = false;
	        }

	        $scope.currentNode = args.node;
	        $scope.currentNode.selected = true;
	        $scope.target.id = args.node.id;
	        $scope.target.name = args.node.name;

	        if (args.node.id < 0) {
	            $scope.target.url = "/";
	        }
	        else {
	            contentResource.getNiceUrl(args.node.id).then(function (url) {
	                $scope.target.url = url;
	            });
	        }

	        if (!angular.isUndefined($scope.target.isMedia)) {
	            delete $scope.target.isMedia;
	        }
	    }

	    $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);

	    $scope.$on('$destroy', function () {
	        $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
	    });
	});