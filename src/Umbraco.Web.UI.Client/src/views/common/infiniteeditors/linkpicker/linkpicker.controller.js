//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Editors.LinkPickerController",
    function ($scope, eventsService, entityResource, contentResource, mediaHelper, userService, localizationService, tinyMceService, editorService) {
        
        var vm = this;
        var dialogOptions = $scope.model;

        var searchText = "Search...";

        vm.submit = submit;
        vm.close = close;

        localizationService.localize("general_search").then(function (value) {
            searchText = value + "...";
        });

        if (!$scope.model.title) {
            localizationService.localize("defaultdialogs_selectLink")
                .then(function(value) {
                    $scope.model.title = value;
                });
        }

        $scope.dialogTreeApi = {};
        $scope.model.target = {};
        $scope.searchInfo = {
            searchFromId: null,
            searchFromName: null,
            showSearch: false,
            results: [],
            selectedSearchResults: []
        };

        $scope.showTarget = $scope.model.hideTarget !== true;

        if (dialogOptions.currentTarget) {
            $scope.model.target = dialogOptions.currentTarget;
            //if we have a node ID, we fetch the current node to build the form data
            if ($scope.model.target.id || $scope.model.target.udi) {

                //will be either a udi or an int
                var id = $scope.model.target.udi ? $scope.model.target.udi : $scope.model.target.id;

                if (!$scope.model.target.path) {

                    entityResource.getPath(id, "Document").then(function (path) {
                        $scope.model.target.path = path;
                        //now sync the tree to this path
                        $scope.dialogTreeApi.syncTree({
                            path: $scope.model.target.path,
                            tree: "content"
                        });
                    });
                }

                // if a link exists, get the properties to build the anchor name list
                contentResource.getById(id).then(function (resp) {
                    $scope.anchorValues = tinyMceService.getAnchorNames(JSON.stringify(resp.properties));
                    $scope.model.target.url = resp.urls[0];
                });
            } else if ($scope.model.target.url.length) {
                // a url but no id/udi indicates an external link - trim the url to remove the anchor/qs
                // only do the substring if there's a # or a ?
                var indexOfAnchor = $scope.model.target.url.search(/(#|\?)/);
                if (indexOfAnchor > -1) {
                    // populate the anchor
                    $scope.model.target.anchor = $scope.model.target.url.substring(indexOfAnchor);
                    // then rewrite the model and populate the link
                    $scope.model.target.url = $scope.model.target.url.substring(0, indexOfAnchor);
                }            }
        } else if (dialogOptions.anchors) {
            $scope.anchorValues = dialogOptions.anchors;
        }

        function nodeSelectHandler(args) {
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
                contentResource.getById(args.node.id).then(function (resp) {
                    $scope.anchorValues = tinyMceService.getAnchorNames(JSON.stringify(resp.properties));
                    $scope.model.target.url = resp.urls[0].text;
                });
            }

            if (!angular.isUndefined($scope.model.target.isMedia)) {
                delete $scope.model.target.isMedia;
            }
        }

        function nodeExpandedHandler(args) {
            // open mini list view for list views
            if (args.node.metaData.isContainer) {
                openMiniListView(args.node);
            }
        }

        $scope.switchToMediaPicker = function () {
            userService.getCurrentUser().then(function (userData) {
                var mediaPicker = {
                    startNodeId: userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0],
                    startNodeIsVirtual: userData.startMediaIds.length !== 1,
                    submit: function (model) {
                        var media = model.selection[0];

                        $scope.model.target.id = media.id;
                        $scope.model.target.udi = media.udi;
                        $scope.model.target.isMedia = true;
                        $scope.model.target.name = media.name;
                        $scope.model.target.url = mediaHelper.resolveFile(media);

                        editorService.close();
                    },
                    close: function() {
                        editorService.close();
                    }
                };
                editorService.mediaPicker(mediaPicker);
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

        $scope.onTreeInit = function () {
            $scope.dialogTreeApi.callbacks.treeNodeSelect(nodeSelectHandler);
            $scope.dialogTreeApi.callbacks.treeNodeExpanded(nodeExpandedHandler);
        }

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

        function close() {
            if($scope.model && $scope.model.close) {
                $scope.model.close();
            }
        }

        function submit() {
            if($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }

    });
