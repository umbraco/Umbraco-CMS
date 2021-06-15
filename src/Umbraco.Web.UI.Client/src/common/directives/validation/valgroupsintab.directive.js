(function () {
  'use strict';

  function valGroupsInTabDirective(angularHelper) {
      function link(scope, el, attrs, ctrl) {
          const valFormManager = ctrl[1];
          const formCtrl = ctrl[0];

          if (!valFormManager) {
            return;
          }

          // valformmanager onValidationStatusChanged only emit updates when the parent form status changes.
          // In this case we need to know the status for all sub forms so we can update each tab validation status correctly based on the child group form states.
          // the code finds the child group form for a given tab and if any of those group forms are invalid the tab will be flag with an error.
          scope.$watch(() => angularHelper.countAllFormErrors(formCtrl), () => {
            update();
          });

          function update () {
            const { tab, groups } = scope.$eval(attrs.valGroupsInTab);

            if (!tab && !groups) {
              tab.hasError = false;
              return;
            }

            const tabGroups = [];
            const tabGroupsIdentifiers = groups.filter(group => group.parentKey === tab.key).map(group => group.key.replaceAll('-', ''));

            for (const [key, value] of Object.entries(formCtrl)) {
              if (key.startsWith('groupForm')) {
                const groupIdentifier = key.replace('groupForm', '');
                if (groupIdentifier && tabGroupsIdentifiers.indexOf(groupIdentifier) !== -1) {
                  tabGroups.push(value);
                }
              }
            }

            tab.hasError = tabGroups.filter(group => group.$invalid).length > 0;
          }
      }

      var directive = {
          require: ['?^^form', '?^^valFormManager'],
          restrict: "A",
          link: link
      };

      return directive;
  }

  angular.module('umbraco.directives').directive('valGroupsInTab', valGroupsInTabDirective);

})();
