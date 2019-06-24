//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Overlays.LinkPickerController",
    function ($scope, eventsService, dialogService, entityResource, mediaHelper, userService, localizationService, tinyMceService) {
        var dialogOptions = $scope.model;

        var searchText = "Search...";
        localizationService.localize("general_search").then(function (value) {
            searchText = value + "...";
        });

        if (!$scope.model.title) {
            $scope.model.title = localizationService.localize("defaultdialogs_selectLink");
        }

        $scope.dialogTreeEventHandler = $({});
        $scope.model.target = {};
        $scope.searchInfo = {
            searchFromId: null,
            searchFromName: null,
            showSearch: false,
            dataTypeId: dialogOptions.dataTypeId,
            results: [],
            selectedSearchResults: []
        };
        $scope.customTreeParams = dialogOptions.dataTypeId ? "dataTypeId=" + dialogOptions.dataTypeId : "";
        $scope.showTarget = $scope.model.hideTarget !== true;

        if (dialogOptions.currentTarget) {
            // clone the current target so we don't accidentally update the caller's model while manipulating $scope.model.target
            $scope.model.target = angular.copy(dialogOptions.currentTarget);
            //if we have a node ID, we fetch the current node to build the form data
            if ($scope.model.target.id || $scope.model.target.udi) {

                //will be either a udi or an int
                var id = $scope.model.target.udi ? $scope.model.target.udi : $scope.model.target.id;

                // is it a content link?
                if (!$scope.model.target.isMedia) {
                    // get the content path
                    entityResource.getPath(id, "Document").then(function (path) {
                        //now sync the tree to this path
                        $scope.dialogTreeEventHandler.syncTree({
                            path: path,
                            tree: "content"
                        });
                    });


                    entityResource.getUrlAndAnchors(id).then(function(resp){
                        $scope.anchorValues = resp.anchorValues;
                        $scope.model.target.url = resp.url;
                    });
                }
            } else if ($scope.model.target.url.length) {
                // a url but no id/udi indicates an external link - trim the url to remove the anchor/qs
                // only do the substring if there's a # or a ?
                var indexOfAnchor = $scope.model.target.url.search(/(#|\?)/);
                if (indexOfAnchor > -1) {
                    // populate the anchor
                    $scope.model.target.anchor = $scope.model.target.url.substring(indexOfAnchor);
                    // then rewrite the model and populate the link
                    $scope.model.target.url = $scope.model.target.url.substring(0, indexOfAnchor);
                }
            }
        } else if (dialogOptions.anchors) {
            $scope.anchorValues = dialogOptions.anchors;
        }

        function nodeSelectHandler(ev, args) {
            if (args && args.event) {
                args.event.preventDefault();
                args.event.stopPropagation();
            }

            eventsService.emit("dialogs.linkPicker.select", args);

            if ($scope.currentNode) {
                //un-select if there's a current one selected
                $scope.currentNode.selected = false;
            }

            $scope.currentNode = args.node;
            $scope.currentNode.selected = true;
            $scope.model.target.id = args.node.id;
            $scope.model.target.udi = args.node.udi;
            $scope.model.target.name = args.node.name;

            if (args.node.id < 0) {
                $scope.model.target.url = "/";
            } else {
                entityResource.getUrlAndAnchors(args.node.id).then(function(resp){
                    $scope.anchorValues = resp.anchorValues;
                    $scope.model.target.url = resp.url;
                });
            }

            if (!angular.isUndefined($scope.model.target.isMedia)) {
                delete $scope.model.target.isMedia;
            }
        }

        function nodeExpandedHandler(ev, args) {
            // open mini list view for list views
            if (args.node.metaData.isContainer) {
                openMiniListView(args.node);
            }
        }

        $scope.switchToMediaPicker = function () {
            userService.getCurrentUser().then(function (userData) {
                var startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
                var startNodeIsVirtual = userData.startMediaIds.length !== 1;

                if (dialogOptions.ignoreUserStartNodes) {
                    startNodeId = -1;
                    startNodeIsVirtual = true;
                }
                $scope.mediaPickerOverlay = {
                    view: "mediapicker",
                    startNodeId: startNodeId,
                    startNodeIsVirtual: startNodeIsVirtual,
                    show: true,
                    dataTypeId: dialogOptions.dataTypeId,
                    submit: function (model) {
                        var media = model.selectedImages[0];

                        $scope.model.target.id = media.id;
                        $scope.model.target.udi = media.udi;
                        $scope.model.target.isMedia = true;
                        $scope.model.target.name = media.name;
                        $scope.model.target.url = mediaHelper.resolveFile(media);

                        $scope.mediaPickerOverlay.show = false;
                        $scope.mediaPickerOverlay = null;

                        // make sure the content tree has nothing highlighted
                        $scope.dialogTreeEventHandler.syncTree({
                            path: "-1",
                            tree: "content"
                        });
                    }
                };
            });
        };

        $scope.hideSearch = function () {
            $scope.searchInfo.showSearch = false;
            $scope.searchInfo.searchFromId = null;
            $scope.searchInfo.searchFromName = null;
            $scope.searchInfo.results = [];
        }

        // method to select a search result
        $scope.selectResult = function (evt, result) {
            result.selected = result.selected === true ? false : true;
            nodeSelectHandler(evt, {
                event: evt,
                node: result
            });
        };

        //callback when there are search results
        $scope.onSearchResults = function (results) {
            $scope.searchInfo.results = results;
            $scope.searchInfo.showSearch = true;
        };

        $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);
        $scope.dialogTreeEventHandler.bind("treeNodeExpanded", nodeExpandedHandler);

        $scope.$on('$destroy', function () {
            $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
            $scope.dialogTreeEventHandler.unbind("treeNodeExpanded", nodeExpandedHandler);
        });

        // Mini list view
        $scope.selectListViewNode = function (node) {
            node.selected = node.selected === true ? false : true;
            nodeSelectHandler({}, {
                node: node
            });
        };

        $scope.closeMiniListView = function () {
            $scope.miniListView = undefined;
        };

        function openMiniListView(node) {
            $scope.miniListView = node;
        }

    });
