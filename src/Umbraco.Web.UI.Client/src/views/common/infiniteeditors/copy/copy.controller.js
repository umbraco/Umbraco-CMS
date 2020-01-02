(function () {
    "use strict";

    function CopyController($scope, localizationService, eventsService, entityHelper) {

        var vm = this;

        vm.labels = {};
        vm.hideSearch = hideSearch;
        vm.selectResult = selectResult;
        vm.onSearchResults = onSearchResults;
        vm.onToggle = toggleHandler;
        vm.submit = submit;
        vm.close = close;

        var dialogOptions = $scope.model;
        var node = dialogOptions.currentNode;

      $scope.model.relateToOriginal = true;
      $scope.model.includeDescendants = true;
      $scope.dialogTreeApi = {};

        vm.searchInfo = {
            searchFromId: null,
            searchFromName: null,
            showSearch: false,
            results: [],
            selectedSearchResults: []
        };

        // get entity type based on the section
        $scope.entityType = entityHelper.getEntityTypeFromSection(dialogOptions.section);

        function onInit() {

            var labelKeys = [
                "general_copy"
            ];

            localizationService.localizeMany(labelKeys).then(function (data) {

                vm.labels.title = data[0];

                setTitle(vm.labels.title);
            });
        }

        function setTitle(value) {
            if (!$scope.model.title) {
                $scope.model.title = value;
            }
        }

        function nodeSelectHandler(args) {
            if (args && args.event) {
                args.event.preventDefault();
                args.event.stopPropagation();
            }

            //eventsService.emit("editors.content.copyController.select", args);

            if ($scope.model.target) {
                //un-select if there's a current one selected
                $scope.model.target.selected = false;
            }

            $scope.model.target = args.node;
            $scope.model.target.selected = true;
        }

        function nodeExpandedHandler(args) {
            // open mini list view for list views
            if (args.node.metaData.isContainer) {
                openMiniListView(args.node);
            }
        }

        function hideSearch() {
            vm.searchInfo.showSearch = false;
            vm.searchInfo.searchFromId = null;
            vm.searchInfo.searchFromName = null;
            vm.searchInfo.results = [];
        }

        // method to select a search result
        function selectResult(evt, result) {
            result.selected = result.selected === true ? false : true;
            nodeSelectHandler({ event: evt, node: result });
        }

        //callback when there are search results
        function onSearchResults(results) {
            vm.searchInfo.results = results;
            vm.searchInfo.showSearch = true;
        }

        $scope.onTreeInit = function () {
            $scope.dialogTreeApi.callbacks.treeNodeSelect(nodeSelectHandler);
            $scope.dialogTreeApi.callbacks.treeNodeExpanded(nodeExpandedHandler);
        }


        // Mini list view
        $scope.selectListViewNode = function (node) {
            node.selected = node.selected === true ? false : true;
            nodeSelectHandler({ node: node });
        };

        $scope.closeMiniListView = function () {
            $scope.miniListView = undefined;
        };

        function openMiniListView(node) {
            $scope.miniListView = node;
        }

        function submit() {
            if ($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }

        function close() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close();
            }
        }

        function toggleHandler(type) {
            // If the relateToOriginal toggle is clicked
            if (type === "relate") {
                $scope.model.relateToOriginal = !$scope.model.relateToOriginal;
            }
            // If the includeDescendants toggle is clicked
            if (type === "descendants") {
                $scope.model.includeDescendants = !$scope.model.includeDescendants;
            }
        }

        onInit();
    }

    angular.module("umbraco").controller("Umbraco.Editors.CopyController", CopyController);

})();
