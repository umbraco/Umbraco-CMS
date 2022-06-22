(function () {
  "use strict";

  angular
    .module("umbraco")
    .controller("Umbraco.PropertyEditors.OEmbedPickerController",
      function Controller($scope, $sce, editorService) {

        const vm = this;

        vm.add = addEmbed;
        vm.edit = editEmbed;
        vm.remove = removeEmbed;
        vm.trustHtml = trustHtml;
        vm.validateMandatory = validateMandatory;

        vm.allowMultiple = $scope.model.config.allowmultiple;

        vm.items = Array.isArray($scope.model.value) ? $scope.model.value : [];

        vm.sortableOptions = {
          axis: 'y',
          containment: 'parent',
          cursor: 'move',
          items: '> .umb-table-row',
          handle: '.handle',
          tolerance: 'pointer'
        };

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

          if (vm.oembedform && vm.oembedform.itemCount) {
            vm.oembedform.itemCount.$setViewValue(vm.items.length);
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

      });

})();
