//used for the media picker dialog
angular.module("umbraco").controller("Umbraco.Editors.LinkPickerController",
    function ($scope, eventsService, entityResource, mediaResource, udiParser, userService, localizationService, editorService) {

        var vm = this;
        var dialogOptions = $scope.model;

        vm.submit = submit;
        vm.close = close;
        vm.toggleOpenInNewWindow = toggleOpenInNewWindow;

        vm.labels = {};
        localizationService.localizeMany(["defaultdialogs_openInNewWindow"]).then(function (data) {
            vm.labels.openInNewWindow = data[0];
        });

        if (!$scope.model.title) {
            localizationService.localize("defaultdialogs_selectLink")
                .then(function (value) {
                    $scope.model.title = value;
                });
        }
        
        $scope.customTreeParams = dialogOptions.dataTypeKey ? "dataTypeKey=" + dialogOptions.dataTypeKey : "";
        $scope.dialogTreeApi = {};
        $scope.model.target = {};
        $scope.searchInfo = {
            searchFromId: null,
            searchFromName: null,
            showSearch: false,
            dataTypeKey: dialogOptions.dataTypeKey,
            results: [],
            selectedSearchResults: []
        };

        $scope.showTarget = $scope.model.hideTarget !== true;
        $scope.showAnchor = $scope.model.hideAnchor !== true;

        // this ensures that we only sync the tree once and only when it's ready
        var oneTimeTreeSync = {
            executed: false,
            treeReady: false,
            sync: function () {
                // don't run this if:
                // - it was already run once
                // - the tree isn't ready yet
                // - the model path hasn't been loaded yet
                if (this.executed || !this.treeReady || !($scope.model.target && $scope.model.target.path)) {
                    return;
                }

                this.executed = true;
                // sync the tree to the model path
                $scope.dialogTreeApi.syncTree({
                    path: $scope.model.target.path,
                    tree: "content"
                });
            }
        };

        if (dialogOptions.currentTarget) {
            // clone the current target so we don't accidentally update the caller's model while manipulating $scope.model.target
            $scope.model.target = Utilities.copy(dialogOptions.currentTarget);
            // if we have a node ID, we fetch the current node to build the form data
            if ($scope.model.target.id || $scope.model.target.udi) {

                // will be either a udi or an int
                var id = $scope.model.target.udi ? $scope.model.target.udi : $scope.model.target.id;

                if ($scope.model.target.udi) {
                    // extract the entity type from the udi and set target.isMedia accordingly
                    var udi = udiParser.parse(id);

                    if (udi && udi.entityType === "media") {
                        $scope.model.target.isMedia = true;
                    } else {
                        delete $scope.model.target.isMedia;
                    }
                }

                if ($scope.model.target.isMedia) {
                    mediaResource.getById(id).then(function (resp) {
                        $scope.model.target.url = resp.mediaLink;
                    });
                } else {
                    // get the content path
                    entityResource.getPath(id, "Document").then(function (path) {
                        $scope.model.target.path = path;
                        oneTimeTreeSync.sync();
                    });

                    entityResource.getUrlAndAnchors(id).then(function (resp) {
                        $scope.anchorValues = resp.anchorValues;
                        $scope.model.target.url = resp.url;
                    });

                }
            } else if ($scope.model.target.url && $scope.model.target.url.length) {
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

            // need to translate the link target ("_blank" or "") into a boolean value for umb-checkbox
            vm.openInNewWindow = $scope.model.target.target === "_blank";
        }
        else if (dialogOptions.anchors) {
            $scope.anchorValues = dialogOptions.anchors;
        }

        function treeLoadedHandler(args) {
            oneTimeTreeSync.treeReady = true;
            oneTimeTreeSync.sync();
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
            }
            else {
                entityResource.getUrlAndAnchors(args.node.id).then(function (resp) {
                    $scope.anchorValues = resp.anchorValues;
                    $scope.model.target.url = resp.url;
                });
            }

            if (!Utilities.isUndefined($scope.model.target.isMedia)) {
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

                var startNodeId, startNodeIsVirtual;
                if (dialogOptions.ignoreUserStartNodes === true) {
                    startNodeId = -1;
                    startNodeIsVirtual = true;
                }
                else {
                    startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
                    startNodeIsVirtual = userData.startMediaIds.length !== 1;
                }

                var mediaPicker = {
                    startNodeId: startNodeId,
                    startNodeIsVirtual: startNodeIsVirtual,
                    dataTypeKey: dialogOptions.dataTypeKey,
                    submit: function (model) {
                        var media = model.selection[0];

                        $scope.model.target.id = media.id;
                        $scope.model.target.udi = media.udi;
                        $scope.model.target.isMedia = true;
                        $scope.model.target.name = media.name;
                        $scope.model.target.url = media.image;

                        editorService.close();

                        // make sure the content tree has nothing highlighted 
                        $scope.dialogTreeApi.syncTree({
                            path: "-1",
                            tree: "content"
                        });
                    },
                    close: function () {
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
            nodeSelectHandler({
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
            $scope.dialogTreeApi.callbacks.treeLoaded(treeLoadedHandler);
            $scope.dialogTreeApi.callbacks.treeNodeSelect(nodeSelectHandler);
            $scope.dialogTreeApi.callbacks.treeNodeExpanded(nodeExpandedHandler);
        }

        // Mini list view
        $scope.selectListViewNode = function (node) {
            node.selected = node.selected === true ? false : true;
            nodeSelectHandler({
                node: node
            });
        };

        $scope.closeMiniListView = function () {
            $scope.miniListView = undefined;
        };

        function openMiniListView(node) {
            $scope.miniListView = node;
        }

        function toggleOpenInNewWindow(model, value) {
            $scope.model.target.target = model ? "_blank" : "";
        }

        function close() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close();
            }
        }

        function submit() {
            if ($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }

    });
