(function () {
  'use strict';

  /**
   * A component to render the property action toggle
   */

  function umbRteBlockController($scope, $compile, $element) {

      var unsubscribe = [];

      var vm = this;

      vm.$onDestroy = onDestroy;
      vm.$onInit = onInit;
      vm.$onChanges = onChanges;

      vm.stylesheet = "";
      vm.blockStyle = "@import \""+vm.stylesheet+"\"";
      vm.view = false;

      function onDestroy() {
        $element[0]._isInitializedUmbBlock = false;
      }

      function onInit() {
        $element[0]._isInitializedUmbBlock = true;
        console.log($element[0])

        var shadowRoot = $element[0].attachShadow({ mode: 'open' });
        shadowRoot.innerHTML =
        `
            <style ng-if="vm.blockStyle">
              {{vm.blockStyle}}
            </style>
            <div class="umb-block-rte--view" ng-if="vm.view" ng-include="vm.view"></div>
            <div class="umb-block-rte__block--view" ng-if="!vm.view">Hello World</div>
        `;
        $compile(shadowRoot)($scope);

      }


      function onChanges(simpleChanges) {
        console.log("block gets change", simpleChanges);
      }

  }

  var umbRteBlockComponent = {
      //templateUrl: 'views/propertyeditors/rte/blocks/umb-rte-block.html',
      bindings: {
          dataUdi: "<"
      },
      controllerAs: 'vm',
      controller: umbRteBlockController
  };

  angular.module('umbraco.directives').component('umbRteBlock', umbRteBlockComponent);

})();
