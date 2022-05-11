(function () {
  "use strict";

  function ContentAppListViewController($scope) {

    var vm = this;

    vm.propertyEditorReadonly = propertyEditorReadonly;

    function propertyEditorReadonly () {
      // check for permission to update
      return !$scope.variantContent.allowedActions.includes('A');
    }
      
  }

  angular.module("umbraco").controller("Umbraco.Editors.Content.Apps.ListViewController", ContentAppListViewController);
})();
