(function () {
  'use strict';

  /**
   * A component to render the editor tab bar
   */
  
  function umbEditorTabBarController() {

    const vm = this;

  }

  const umbEditorTabBarComponent = {
    templateUrl: 'views/components/editor/umb-editor-tab-bar.html',
    controllerAs: 'vm',
    transclude: true,
    controller: umbEditorTabBarController
  };

  angular.module('umbraco.directives').component('umbEditorTabBar', umbEditorTabBarComponent);
})();
