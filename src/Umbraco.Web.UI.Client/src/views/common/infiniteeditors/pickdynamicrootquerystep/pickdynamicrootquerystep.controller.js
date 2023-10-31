(function () {
    "use strict";

    function PickDynamicRootQueryStepController($scope, localizationService, editorService, udiParser) {

        var vm = this;

        function onInit() {
            if(!$scope.model.title) {
                localizationService.localize("dynamicRoot_pickDynamicRootQueryStepTitle").then(function(value){
                    $scope.model.title = value;
                });
            }
            if(!$scope.model.subtitle) {
                localizationService.localize("dynamicRoot_pickDynamicRootQueryStepDesc").then(function(value){
                    $scope.model.subtitle = value;
                });
            }
        }

        vm.choose = function(queryStepAlias) {
          var editor = {
            multiPicker: true,
            filterCssClass: "not-allowed not-published",
            filter: function (item) {
                // filter out folders (containers), element types (for content)
                return item.nodeType === "container" || item.metaData.isElement;
            },
            submit: function (model) {
                var typeKeys = _.map(model.selection, function(selected) { return udiParser.parse(selected.udi).value; });
                $scope.model.value = {
                  alias: queryStepAlias,
                  anyOfDocTypeKeys: typeKeys
                }
                editorService.close();
                vm.submit($scope.model);
            },
            close: function() {
                editorService.close();
            }
          };

          switch ($scope.model.contentType) {
            case "content":
              editorService.contentTypePicker(editor);
              break;
            case "media":
              editorService.mediaTypePicker(editor);
              break;
          }
        }

        vm.chooseCustom = function() {
          var customStepPicker = {
            view: "views/common/infiniteeditors/pickdynamicrootcustomstep/pickdynamicrootcustomstep.html",
            size: "small",
            value: "",
            submit: function(model) {
              $scope.model.value = {
                alias: model.value
              }
              editorService.close();
              vm.submit($scope.model);
            },
            close: function() {
              editorService.close();
            }
          };
          editorService.open(customStepPicker);
        }

        vm.submit = submit;
        function submit(model) {
          if ($scope.model.submit) {
              $scope.model.submit(model);
          }
        }

        vm.close = close;
        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.PickDynamicRootQueryStep", PickDynamicRootQueryStepController);
})();
