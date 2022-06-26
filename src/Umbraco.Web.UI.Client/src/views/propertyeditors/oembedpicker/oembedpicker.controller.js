(function () {
  "use strict";

  angular
    .module("umbraco")
    .controller("Umbraco.PropertyEditors.OEmbedPickerController",
      function Controller($scope, $sce, editorService) {

        const vm = this;

        let unsubscribe = [];

        vm.add = addEmbed;
        vm.edit = editEmbed;
        vm.remove = removeEmbed;
        vm.trustHtml = trustHtml;
        vm.validateMandatory = validateMandatory;

        vm.validationLimit = $scope.model.config.validationLimit || {};

        // If single-mode we only allow 1 item as the maximum.
        if ($scope.model.config.multiple === false) {
          vm.validationLimit.max = 1;
        }

        vm.singleMode = vm.validationLimit.max === 1;

        vm.items = Array.isArray($scope.model.value) ? $scope.model.value : [];

        vm.sortableOptions = {
          //containment: "parent",
          cursor: "grabbing",
          handle: ".handle", //".umb-media-card",
          //cancel: "input,textarea,select,option",
          items: '.umb-media-card',
          classes: ".umb-media-card--dragging",
          distance: 5,
          tolerance: "pointer",
          update: function (ev, ui) {
            setDirty();
          }
        };

        function setDirty() {
          if (vm.oembedForm) {
            vm.oembedForm.$setDirty();
          }
        }

        function openEmbedDialog(embed, onSubmit) {

          // Pass in a clone of embed object to embed infinite editor.
          const clone = Utilities.copy(embed);

          const embedDialog = {
            modify: clone,
            submit: model => {
              onSubmit(model.embed);
              editorService.close();
            },
            close: () => {
              editorService.close();
            }
          };

          editorService.embed(embedDialog);
        }

        function trustHtml(html) {
          return $sce.trustAsHtml(html);
        }

        function addEmbed(evt) {
          evt.preventDefault();

          openEmbedDialog({},
            (newEmbed) => {
              vm.items.push({
                'url': newEmbed.url,
                'width': newEmbed.width,
                'height': newEmbed.height,
                'preview': newEmbed.preview
              });
              updateModelValue();
            });
        }

        function editEmbed(index, evt) {
          evt.preventDefault();

          var embed = vm.items[index];

          openEmbedDialog(embed,
            (newEmbed) => {

              vm.items[index] = {
                'url': newEmbed.url,
                'width': newEmbed.width,
                'height': newEmbed.height,
                'preview': newEmbed.preview
              };

              updateModelValue();
            });
        }

        function removeEmbed(index, evt) {
          evt.preventDefault();

          vm.items.splice(index, 1);

          updateModelValue();
        }

        function updateModelValue() {
          $scope.model.value = vm.items;

          if (vm.oembedForm && vm.oembedForm.itemCount) {
            vm.oembedForm.itemCount.$setViewValue(vm.items.length);
          }
        }

        function validateMandatory() {
          var isValid = true;

          if ($scope.model.validation.mandatory && (Array.isArray(vm.items) === false || vm.items.length === 0)) {
            isValid = false;
          }

          return {
            isValid: isValid,
            errorMsg: "Value cannot be empty",
            errorKey: "required"
          };
        }

        function onAmountOfMediaChanged() {

          // enable/disable property actions
          //if (copyAllMediasAction) {
          //  copyAllMediasAction.isDisabled = vm.model.value.length === 0;
          //}
          //if (removeAllMediasAction) {
          //  removeAllMediasAction.isDisabled = vm.model.value.length === 0;
          //}

          updateModelValue();

          // validate limits:
          if (vm.oembedForm && vm.validationLimit) {

            var isMinRequirementGood = vm.validationLimit.min === null || $scope.model.value.length >= vm.validationLimit.min;
            vm.oembedForm.minCount.$setValidity("minCount", isMinRequirementGood);

            var isMaxRequirementGood = vm.validationLimit.max === null || $scope.model.value.length <= vm.validationLimit.max;
            vm.oembedForm.maxCount.$setValidity("maxCount", isMaxRequirementGood);
          }
        }

        unsubscribe.push($scope.$watch(() => $scope.model.value.length, onAmountOfMediaChanged));

        $scope.$on("$destroy", function () {
          for (const subscription of unsubscribe) {
            subscription();
          }
        });

      });

})();
