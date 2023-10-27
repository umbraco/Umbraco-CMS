 (function() {
	"use strict";

	function MoveController($scope, localizationService, entityHelper) {

      var vm = this;

      vm.hideSearch = hideSearch;
      vm.selectResult = selectResult;
      vm.onSearchResults = onSearchResults;
      vm.submit = submit;
      vm.close = close;

      var dialogOptions = $scope.model;
      var searchText = "Search...";
      var node = dialogOptions.currentNode;

      $scope.model.relateToOriginal = true;
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

            if (!$scope.model.title) {
                localizationService.localize("actions_move").then(function (value) {
                    $scope.model.title = value;
                });
            }

            localizationService.localize("general_search").then(function (value) {
                searchText = value + "...";
            });

        }

      function nodeSelectHandler(args) {

          if(args && args.event) {
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

      onInit();

	}

	angular.module("umbraco").controller("Umbraco.Editors.MoveController", MoveController);

})();
