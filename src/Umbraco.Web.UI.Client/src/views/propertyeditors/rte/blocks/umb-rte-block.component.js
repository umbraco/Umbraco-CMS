(function () {
  'use strict';

  /**
   * A component to render the property action toggle
   */

  function umbRteBlockController($scope, $compile, $element) {

      var unsubscribe = [];

      var model = this;

      model.$onDestroy = onDestroy;
      model.$onInit = onInit;
      model.$onChanges = onChanges;


      function onDestroy() {
        $element[0]._isInitializedUmbBlock = false;
      }

      function onInit() {
        $element[0]._isInitializedUmbBlock = true;
        $scope.block = $element[0].$block;
        $scope.api = $element[0].$api;
        $scope.index = $element[0].$index;

        //$scope.api = model.api;
        //$scope.index = model.index;

        const stylesheet = $scope.block.config.stylesheet;
        console.log("$scope.block", $scope.block)
        console.log("$scope.block stylesheet", stylesheet)

        var shadowRoot = $element[0].attachShadow({ mode: 'open' });
          shadowRoot.innerHTML =
          `
              <style>@import "assets/css/icons.css?umb__rnd=${Umbraco.Sys.ServerVariables.application.cacheBuster}"</style>
              <style>
                .umb-block-rte--view {
                  position: relative;
                }
                .umb-block-rte--view::after {
                  position:absolute;
                  content: '';
                  inset: 0;
                  border-style: solid;
                  border-color: #6ab4f0;
                  border-color: -webkit-focus-ring-color;
                  border-width: calc(var(--umb-rte-block--selected, 0) * 2px);
                }
              </style>

              ${ stylesheet ? `
                  <style>@import "${stylesheet}?umb__rnd=${Umbraco.Sys.ServerVariables.application.cacheBuster}"</style>`
                  : ''
              }

              <div
                  class="umb-block-rte--view"
                  ng-class="{'show-validation': api.internal.showValidation}" ng-include="block.view">
              </div>
          `;
        $compile(shadowRoot)($scope);

      }


      function onChanges(simpleChanges) {
        console.log("block gets change", simpleChanges);
        // TODO Make sure to fire something to update/reset $block on data-udi change.
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

})();
