 (function() {
	"use strict";

	function CopyOverlay($scope, localizationService, eventsService, entityHelper) {

      var vm = this;

      if(!$scope.model.title) {
          $scope.model.title = localizationService.localize("general_copy");
      }

      vm.hideSearch = hideSearch;
      vm.selectResult = selectResult;
      vm.onSearchResults = onSearchResults;

      var dialogOptions = $scope.model;
      var searchText = "Search...";
      var node = dialogOptions.currentNode;

      localizationService.localize("general_search").then(function (value) {
          searchText = value + "...";
      });

      $scope.model.relateToOriginal = true;
      $scope.dialogTreeEventHandler = $({});

      vm.searchInfo = {
          searchFromId: null,
          searchFromName: null,
          showSearch: false,
          results: [],
          selectedSearchResults: []
      };

      // get entity type based on the section
      $scope.entityType = entityHelper.getEntityTypeFromSection(dialogOptions.section);

      function nodeSelectHandler(ev, args) {
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

      function nodeExpandedHandler(ev, args) {
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
          nodeSelectHandler(evt, { event: evt, node: result });
      }

      //callback when there are search results
      function onSearchResults(results) {
          vm.searchInfo.results = results;
          vm.searchInfo.showSearch = true;
      }

      $scope.dialogTreeEventHandler.bind("treeNodeSelect", nodeSelectHandler);
      $scope.dialogTreeEventHandler.bind("treeNodeExpanded", nodeExpandedHandler);

      $scope.$on('$destroy', function () {
          $scope.dialogTreeEventHandler.unbind("treeNodeSelect", nodeSelectHandler);
          $scope.dialogTreeEventHandler.unbind("treeNodeExpanded", nodeExpandedHandler);
      });

      // Mini list view
      $scope.selectListViewNode = function (node) {
          node.selected = node.selected === true ? false : true;
		  nodeSelectHandler({}, { node: node });
      };

      $scope.closeMiniListView = function () {
          $scope.miniListView = undefined;
      };

      function openMiniListView(node) {
          $scope.miniListView = node;
      }


	}

	angular.module("umbraco").controller("Umbraco.Overlays.CopyOverlay", CopyOverlay);

})();
