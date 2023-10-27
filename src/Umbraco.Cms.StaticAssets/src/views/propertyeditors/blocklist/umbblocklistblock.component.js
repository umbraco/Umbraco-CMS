(function () {
  "use strict";

  /**
   * @ngdoc directive
   * @name umbraco.directives.directive:umbBlockListBlock
   * @description
   * The component to render the view for a block.
   * If a stylesheet is used then this uses a ShadowDom to make a scoped element.
   * This way the backoffice styling does not collide with the block style.
   */
  
  angular
    .module("umbraco")
    .component("umbBlockListBlock", {
      controller: BlockListBlockController,
      controllerAs: "model",
      bindings: {
        stylesheet: "@",
        view: "@",
        block: "=",
        api: "<",
        index: "<",
        parentForm: "<"
      },
      require: {
        valFormManager: "^^valFormManager"
      }
    }
  );

  function BlockListBlockController($scope, $compile, $element) {
    var model = this;

    model.$onInit = function () {
      // This is ugly and is only necessary because we are not using components and instead
      // relying on ng-include. It is definitely possible to compile the contents
      // of the view into the DOM using $templateCache and $http instead of using
      // ng - include which means that the controllerAs flows directly to the view.
      // This would mean that any custom components would need to be updated instead of relying on $scope.
      // Guess we'll leave it for now but means all things need to be copied to the $scope and then all
      // primitives need to be watched.

      // let the Block know about its form
      model.block.setParentForm(model.parentForm);

      // let the Block know about the current index
      model.block.index = model.index;

      $scope.block = model.block;
      $scope.api = model.api;
      $scope.index = model.index;
      $scope.parentForm = model.parentForm;
      $scope.valFormManager = model.valFormManager;

      if (model.stylesheet) {
        var shadowRoot = $element[0].attachShadow({ mode: 'open' });
        shadowRoot.innerHTML = `
                    <style>
                    @import "${model.stylesheet}"
                    </style>
                    <div class="umb-block-list__block--view" ng-include="'${model.view}'"></div>
                `;
        $compile(shadowRoot)($scope);
      }
      else {
        $element.append($compile('<div class="umb-block-list__block--view" ng-include="model.view"></div>')($scope));
      }
    };

    // We need to watch for changes on primitive types and update the $scope values.
    model.$onChanges = function (changes) {
      if (changes.index) {
        var index = changes.index.currentValue;
        $scope.index = index;

        // let the Block know about the current index:
        if ($scope.block) {
          $scope.block.index = index;
        }
      }
    };
  }


})();
