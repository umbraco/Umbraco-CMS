 (function() {
	"use strict";

	function MoveOverlay($scope, localizationService, eventsService) {

      var vm = this;

      vm.hideSearch = hideSearch;
      vm.selectResult = selectResult;
      vm.onSearchResults = onSearchResults;

      var dialogOptions = $scope.model;
      var searchText = "Search...";
      var node = dialogOptions.currentNode;

      localizationService.localize("general_search").then(function (value) {
          searchText = value + "...";
      });

      if(!$scope.model.title) {
          $scope.model.title = localizationService.localize("actions_move");
      }

      $scope.model.relateToOriginal = true;
      $scope.dialogTreeEventHandler = $({});

      vm.searchInfo = {
          searchFromId: null,
          searchFromName: null,
          showSearch: false,
          results: [],
          selectedSearchResults: []
      };

      function nodeSelectHandler(ev, args) {
          args.event.preventDefault();
          args.event.stopPropagation();

          if (args.node.metaData.listViewNode) {
              //check if list view 'search' node was selected

              vm.searchInfo.showSearch = true;
              vm.searchInfo.searchFromId = args.node.metaData.listViewNode.id;
              vm.searchInfo.searchFromName = args.node.metaData.listViewNode.name;
          }
          else {
              //eventsService.emit("editors.content.copyController.select", args);

              if ($scope.model.target) {
                  //un-select if there's a current one selected
                  $scope.model.target.selected = false;
              }

              $scope.model.target = args.node;
              $scope.model.target.selected = true;
          }

      }

      function nodeExpandedHandler(ev, args) {
          if (angular.isArray(args.children)) {

              //iterate children
              _.each(args.children, function (child) {
                  //check if any of the items are list views, if so we need to add a custom
                  // child: A node to activate the search
                  if (child.metaData.isContainer) {
                      child.hasChildren = true;
                      child.children = [
                          {
                              level: child.level + 1,
                              hasChildren: false,
                              name: searchText,
                              metaData: {
                                  listViewNode: child,
                              },
                              cssClass: "icon umb-tree-icon sprTree icon-search",
                              cssClasses: ["not-published"]
                          }
                      ];
                  }
              });
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


	}

	angular.module("umbraco").controller("Umbraco.Overlays.MoveOverlay", MoveOverlay);

})();
