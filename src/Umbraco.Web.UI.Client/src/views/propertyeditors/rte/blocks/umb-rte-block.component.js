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

        const stylesheet = $scope.block.config.stylesheet + `?umb__rnd=${Umbraco.Sys.ServerVariables.application.cacheBuster}`;
        console.log("$scope.block", $scope.block)
        console.log("$scope.block stylesheet", stylesheet)

        var shadowRoot = $element[0].attachShadow({ mode: 'open' });
        shadowRoot.innerHTML =
        (stylesheet ? `<style>
          @import "${stylesheet}"
        </style>` : '') +
        `
            <div class="umb-block-rte--view" ng-include="block.view"></div>
        `;
        $compile(shadowRoot)($scope);

      }


      function onChanges(simpleChanges) {
        console.log("block gets change", simpleChanges);
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
