(function () {
  "use strict";

  function ContentAppListViewController($scope) {

    var vm = this;

    vm.propertyEditorReadonly = propertyEditorReadonly;

    function propertyEditorReadonly () {
      const allowBrowse = $scope.variantContent.allowedActions.includes('F');
      return allowBrowse && $scope.variantContent.allowedActions.length === 1;
    }
      
  }

  angular.module("umbraco").controller("Umbraco.Editors.Content.Apps.ListViewController", ContentAppListViewController);
})();
