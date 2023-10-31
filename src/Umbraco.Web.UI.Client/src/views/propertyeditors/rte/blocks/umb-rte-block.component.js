(function () {
  'use strict';

  /**
   * A component to render the property action toggle
   */

  function umbRteBlockController($scope, $compile, $element) {

      var model = this;

      model.$onDestroy = onDestroy;
      model.$onInit = onInit;


      function onDestroy() {
        $element[0]._isInitializedUmbBlock = false;
      }

      function onInit() {
        $element[0]._isInitializedUmbBlock = true;
        $scope.block = $element[0].$block;
        $scope.api = $element[0].$api;
        $scope.index = $element[0].$index;
        $scope.culture = $element[0].$culture || null;
        $scope.segment = $element[0].$segment || null;
        $scope.parentForm = $element[0].$parentForm;
        $scope.valFormManager = $element[0].$valFormManager;

        const stylesheet = $scope.block.config.stylesheet;

        var shadowRoot = $element[0].attachShadow({ mode: 'open' });
          shadowRoot.innerHTML =
          `
              <style>
                @import "assets/css/icons.css?umb__rnd=${Umbraco.Sys.ServerVariables.application.cacheBuster}";
                @import "assets/css/blockrteui.css?umb__rnd=${Umbraco.Sys.ServerVariables.application.cacheBuster}";
                ${ stylesheet ? `@import "${stylesheet}?umb__rnd=${Umbraco.Sys.ServerVariables.application.cacheBuster}";` : ''}

                :host {
                  display: ${$element[0].nodeName.toLowerCase() === 'umb-rte-block-inline' ? 'inline-block' : 'block'};
                  user-select:none;
                  user-drag:auto;
                }

                .umb-block-rte__block--actions {
                  opacity: 0;
                  transition: opacity 120ms;
                }

                :host(:focus) .umb-block-rte__block--actions,
                :host(:focus-within) .umb-block-rte__block--actions,
                :host(:hover) .umb-block-rte__block--actions {
                  opacity: 1;
                }

              </style>

              <div class="umb-block-rte__block">
                <ng-form name="model.blockForm" val-server-match="{ 'contains' : { 'valServerMatchContent': block.content.key, 'valServerMatchSettings': block.settings.key }, 'culture': culture, 'segment': segment, }">

                  <div
                      class="umb-block-rte--view"
                      ng-class="{'show-validation': api.internal.showValidation}" ng-include="block.view">
                  </div>

                  <div class="umb-block-rte__block--actions">

                    <button type="button" class="btn-reset action --content" localize="title" title="actions_editContent"
                            ng-click="api.editBlock(block, false, index, model.blockForm);"
                            ng-if="block.showContent === true"
                            ng-class="{ '--error': model.blockForm.$error.valServerMatchContent }">
                        <umb-icon icon="icon-edit" class="icon"></umb-icon>
                        <span class="sr-only">
                            <localize key="general_content">Content</localize>
                        </span>
                        <div class="__error-badge">!</div>
                    </button>

                    <button type="button" class="btn-reset action --settings" localize="title" title="actions_editSettings"
                            ng-click="api.openSettingsForBlock(block, index, model.blockForm);"
                            ng-class="{ '--error': model.blockForm.$error.valServerMatchSettings }"
                            ng-if="block.showSettings === true">
                        <umb-icon icon="icon-settings" class="icon"></umb-icon>
                        <span class="sr-only">
                            <localize key="general_settings">Settings</localize>
                        </span>
                        <div class="__error-badge">!</div>
                    </button>
                    <button type="button" class="btn-reset action --copy" localize="title" title="actions_copy"
                            ng-click="api.copyBlock(block);"
                            ng-if="block.showCopy === true">
                        <umb-icon icon="icon-documents" class="icon"></umb-icon>
                        <span class="sr-only">
                            <localize key="general_copy">Copy</localize>
                        </span>
                    </button>
                    <button ng-if="!api.readonly"
                            type="button" class="btn-reset action --delete" localize="title" title="actions_delete"
                            ng-click="api.requestDeleteBlock(block);">
                        <umb-icon icon="icon-trash" class="icon"></umb-icon>
                        <span class="sr-only">
                            <localize key="general_delete">Delete</localize>
                        </span>
                    </button>
                  </div>
              </ng-form>
            </div>
          `;
        $compile(shadowRoot)($scope);

      }

  }

  var umbRteBlockComponent = {
      bindings: {
          dataUdi: "<"
      },
      controller: umbRteBlockController,
      controllerAs: "model"
  };

  angular.module('umbraco.directives').component('umbRteBlock', umbRteBlockComponent);
  angular.module('umbraco.directives').component('umbRteBlockInline', umbRteBlockComponent);

})();
