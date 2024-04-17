//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
.controller("Umbraco.PrevalueEditors.TreeSourceController",

  function ($scope, $filter, $timeout, $q, entityResource, angularHelper, iconHelper, editorService, eventsService, localizationService, udiService, udiParser) {

    const vm = this;

    vm.clear = clear;
    vm.clearXPath = clearXPath;
    vm.clearDynamicStartNode = clearDynamicStartNode;
    vm.chooseDynamicStartNode = chooseDynamicStartNode;
    vm.chooseXPath = chooseXPath;
    vm.openContentPicker = openContentPicker;
    vm.openDynamicRootOriginPicker = openDynamicRootOriginPicker;
    vm.appendDynamicQueryStep = appendDynamicQueryStep;
    vm.removeQueryStep = removeQueryStep;

    vm.dynamicRootOrigin = null;
    vm.querySteps = [];
    vm.sortableModel = [];

    vm.sortableOptionsForQuerySteps = {
      axis: "y",
      containment: "parent",
      distance: 10,
      opacity: 0.7,
      tolerance: "pointer",
      scroll: true,
      zIndex: 6000,
      update: function (e, ui) {
        setDirty();
      }
    };

    vm.showDynamicStartNode = false;
    vm.showXPath = false;

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

    if ($scope.model.value.dynamicRoot && $scope.model.value.dynamicRoot.querySteps) {

      vm.sortableModel = $scope.model.value.dynamicRoot.querySteps;

      syncRenderModel();
    }

    if ($scope.model.value.id && $scope.model.value.type !== "member") {
      entityResource.getById($scope.model.value.id, entityType()).then(item => {
          populate(item);
      });
    } else {
      $timeout(function () {
          treeSourceChanged();
      }, 100);
    }

    function entityType() {
      var ent = "Document";
      if ($scope.model.value.type === "media"){
          ent = "Media";
      }
      else if ($scope.model.value.type === "member") {
          ent = "Member";
      }
      return ent;
    }

    function setDirty() {
      const currentForm = angularHelper.getCurrentForm($scope);
      if (currentForm) {
        currentForm.$setDirty();
      }
    }

    function openContentPicker() {
			const treePicker = {
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
		}

    function chooseXPath() {
			vm.showXPath = true;
      $scope.model.value.dynamicRoot = null;
    }

    function chooseDynamicStartNode() {
			vm.showXPath = false;
      $scope.model.value.dynamicRoot = {
        originAlias: "Parent",
        querySteps: []
      };
		}

    function clearXPath() {
      vm.showXPath = false;
      $scope.model.value.query = null;
    }

    function clearDynamicStartNode() {
      vm.showDynamicStartNode = false;
      vm.querySteps = [];
      vm.sortableModel = [];
      $scope.model.value.dynamicRoot = null;
      setSortingState(vm.querySteps);
    }

    function clear() {
			$scope.model.value.id = null;
      $scope.node = null;
      $scope.model.value.query = null;
      $scope.model.value.dynamicRoot = null;
      treeSourceChanged();
		}

    function treeSourceChanged() {
      eventsService.emit("treeSourceChanged", { value: $scope.model.value.type });
    }

		//we always need to ensure we dont submit anything broken
    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
      if ($scope.model.value.type === "member") {
        $scope.model.value.id = null;
        $scope.model.value.query = "";
        $scope.model.value.dynamicRoot = null;
      }
    });

    //when the scope is destroyed we need to unsubscribe
    $scope.$on('$destroy', function () {
      unsubscribe();
    });

		function populate(item) {
			clear();
			item.icon = iconHelper.convertFromLegacyIcon(item.icon);
			$scope.node = item;
      $scope.node.path = "";
      $scope.model.value.id = $scope.model.config.idType === "udi" ? item.udi : item.id;
      entityResource.getUrl(item.id, entityType()).then(function (data) {
          $scope.node.path = data;
      });
		}

    // Dynamic Root specific
    $scope.$watch("model.value.dynamicRoot", function (newVal, oldVal) {

      const alias = newVal ? newVal.originAlias : null;
      const originKey = newVal ? newVal.originKey : null;

      if (!alias) {
        vm.dynamicRootOrigin = null;
        return;
      }

      const icon = getIconForOriginAlias(alias);
      const key = `dynamicRoot_origin${alias}Title`;

      vm.dynamicRootOrigin = {
        alias: alias,
        icon: icon
      };

      setSortingState(newVal.querySteps);

      if (originKey) {
        const lookupId = udiService.build($scope.model.value.type === 'content' ? 'document' : $scope.model.value.type, originKey);

        vm.dynamicRootOrigin.description = "Loading...";
        //vm.dynamicRootOrigin.description = $filter('ncNodeName')(lookupId);

        const udi = udiParser.parse(lookupId);
        if (udi) {
          entityResource.getById(udi.value, udi.entityType).then(ent => {
            vm.dynamicRootOrigin.description = ent.name;
          })
        }

      }

      localizationService.localize(key).then(data => {
        vm.dynamicRootOrigin.name = data;
      });

    });

    $scope.$watchCollection("vm.sortableModel", function (newVal, oldVal) {
      if (newVal !== oldVal) {
        $scope.model.value.dynamicRoot.querySteps = newVal;

        syncRenderModel();
      }
    });

    function syncRenderModel() {
      const promises = [];

      $scope.model.value.dynamicRoot.querySteps.forEach(x => {
        promises.push(getDataForQueryStep(x));
      });

      $q.all(promises).then(data => {
        vm.querySteps = data;
        setSortingState(vm.querySteps);
      });
    }

    function getIconForOriginAlias(originAlias) {
      switch (originAlias) {
        case "Root":
          return "icon-home";
        case "Parent":
          return "icon-page-up";
        case "Current":
          return "icon-document";
        case "Site":
          return "icon-home";
        case "ByKey":
          return "icon-wand";
      }
    }

    function getIconForQueryStep(queryStep) {
      switch (queryStep.alias) {
        case "NearestAncestorOrSelf":
          return "icon-chevron-up";
        case "FurthestAncestorOrSelf":
          return "icon-chevron-up";
        case "NearestDescendantOrSelf":
          return "icon-chevron-down";
        case "FurthestDescendantOrSelf":
          return "icon-chevron-down";
      }
      return "icon-lab";
    }

    function getNameKeyForQueryStep(queryStep) {

      let key = "";

      switch (queryStep.alias) {
        case "NearestAncestorOrSelf":
        case "FurthestAncestorOrSelf":
        case "NearestDescendantOrSelf":
        case "FurthestDescendantOrSelf":
          key = `dynamicRoot_queryStep${queryStep.alias}Title`;
      }

      return key;
    }

    function getDataForQueryStep(queryStep) {
      const deferred = $q.defer();

      const icon = getIconForQueryStep(queryStep);
      let nameKey = getNameKeyForQueryStep(queryStep);

      const keys = [
        nameKey,
        "dynamicRoot_queryStepTypes"
      ];

      localizationService.localizeMany(keys).then(values => {

        const name = values[0] === "[]" ? queryStep.alias : values[0];
        let description = null;

        if (queryStep.anyOfDocTypeKeys && queryStep.anyOfDocTypeKeys.length > 0) {
          description = (values[1] || "That matches types: ") + queryStep.anyOfDocTypeKeys.join(", ")
        }

        const obj = {
          alias: queryStep.alias,
          name: name,
          description: description,
          icon: icon
        };

        deferred.resolve(obj);

      });

      return deferred.promise;
    }

    function removeQueryStep(queryStep, index) {
      if (index !== -1) {
        $scope.model.value.dynamicRoot.querySteps.splice(index, 1);
        vm.sortableModel = $scope.model.value.dynamicRoot.querySteps;
        vm.querySteps.splice(index, 1);
        setSortingState(vm.querySteps);
      }
    }

    function setSortingState(items) {
      // disable sorting if the list only consist of one item
      if (items.length <= 1 || $scope.readonly) {
        vm.sortableOptionsForQuerySteps.disabled = true;
      } else {
        vm.sortableOptionsForQuerySteps.disabled = false;
      }
    }

    function openDynamicRootOriginPicker() {
			const originPicker = {
        view: "views/common/infiniteeditors/pickdynamicrootorigin/pickdynamicrootorigin.html",
        contentType: $scope.model.value.type,
        size: "small",
        value: {...$scope.model.value.dynamicRoot},
				multiPicker: false,
				submit: function(model) {
          $scope.model.value.dynamicRoot = model.value;
					editorService.close();
				},
				close: function() {
					editorService.close();
				}
			};
			editorService.open(originPicker);
		}

    function appendDynamicQueryStep() {
      const queryStepPicker = {
        view: "views/common/infiniteeditors/pickdynamicrootquerystep/pickdynamicrootquerystep.html",
        contentType: $scope.model.value.type,
        size: "small",
				multiPicker: false,
				submit: function(model) {
          if (!$scope.model.value.dynamicRoot.querySteps) {
            $scope.model.value.dynamicRoot.querySteps = [];
          }

          $scope.model.value.dynamicRoot.querySteps.push(model.value);

          vm.sortableModel = $scope.model.value.dynamicRoot.querySteps;

          const promises = [
            getDataForQueryStep(model.value)
          ];

          $q.all(promises).then(data => {
            vm.querySteps.push(data[0]);
            setSortingState(vm.querySteps);
          });

					editorService.close();
				},
				close: function() {
					editorService.close();
				}
			};
			editorService.open(queryStepPicker);
    }

});
