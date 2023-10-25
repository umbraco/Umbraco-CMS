//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.PrevalueEditors.TreeSourceController",

	function($scope, $timeout, entityResource, iconHelper, editorService, eventsService){

    $scope.showXPath = false;

    if (!$scope.model) {
        $scope.model = {};
    }
    if (!$scope.model.value) {
      $scope.model.value = {
          type: "content"
      };
    }
    if (!$scope.model.config) {
      $scope.model.config = {
          idType: "udi"
      };
    }

    if($scope.model.value.id && $scope.model.value.type !== "member"){
      entityResource.getById($scope.model.value.id, entityType()).then(function(item){
          populate(item);
      });
    } else {
      $timeout(function () {
          treeSourceChanged();
      }, 100);
    }

    function entityType() {
      var ent = "Document";
      if($scope.model.value.type === "media"){
          ent = "Media";
      }
      else if ($scope.model.value.type === "member") {
          ent = "Member";
      }
      return ent;
    }

		$scope.openContentPicker = function() {
			var treePicker = {
        idType: $scope.model.config.idType,
				section: $scope.model.value.type,
				treeAlias: $scope.model.value.type,
				multiPicker: false,
				submit: function(model) {
					var item = model.selection[0];
					populate(item);
					editorService.close();
				},
				close: function() {
					editorService.close();
				}
			};
			editorService.treePicker(treePicker);
		};

		$scope.chooseXPath = function() {
			$scope.showXPath = true;
      $scope.model.value.queryFilter = null;
		};
		$scope.chooseDynamicStartNode = function() {
			$scope.showXPath = false;
      $scope.model.value.queryFilter = {
        originAlias: "Parent",
        filter: []
      };
		};

		$scope.clearXPath = function() {
      $scope.model.value.query = null;
			$scope.showXPath = false;
		};
		$scope.clearDynamicStartNode = function() {
      $scope.model.value.queryFilter = null;
			$scope.showDynamicStartNode = false;
		};

		$scope.clear = function() {
			$scope.model.value.id = null;
      $scope.node = null;
      $scope.model.value.query = null;
      $scope.model.value.queryFilter = null;
      treeSourceChanged();
		};

    function treeSourceChanged() {
      eventsService.emit("treeSourceChanged", { value: $scope.model.value.type });
    }

		//we always need to ensure we dont submit anything broken
    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
      if($scope.model.value.type === "member"){
        $scope.model.value.id = null;
        $scope.model.value.query = "";
        $scope.model.value.queryFilter = null;
      }
    });

    //when the scope is destroyed we need to unsubscribe
    $scope.$on('$destroy', function () {
      unsubscribe();
    });

		function populate(item) {
			$scope.clear();
			item.icon = iconHelper.convertFromLegacyIcon(item.icon);
			$scope.node = item;
      $scope.node.path = "";
      $scope.model.value.id = $scope.model.config.idType === "udi" ? item.udi : item.id;
      entityResource.getUrl(item.id, entityType()).then(function (data) {
          $scope.node.path = data;
      });
		}


    // Dynamic Root specific:

    $scope.openDynamicRootOriginPicker = function() {
			var originPicker = {
        view: "views/common/infiniteeditors/pickdynamicrootorigin/pickdynamicrootorigin.html",
        contentType: $scope.model.value.type,
        size: "small",
        value: {...$scope.model.value.queryFilter},
				multiPicker: false,
				submit: function(model) {
					$scope.model.value.queryFilter = model.value;
					editorService.close();
				},
				close: function() {
					editorService.close();
				}
			};
			editorService.open(originPicker);
		};

    $scope.appendDynamicQueryStep = function () {
      $scope.model.value.queryFilter.push({
        directionAlias: "NearestDescendantOrSelf",
        anyOfDocTypeAlias:[
          "stages"
        ]
      });
    }
});
