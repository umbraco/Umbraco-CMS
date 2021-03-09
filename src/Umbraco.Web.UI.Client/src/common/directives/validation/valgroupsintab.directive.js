(function () {
  'use strict';

  function valGroupsInTabDirective() {
      function link(scope, el, attrs, ctrl) {
          const valFormManager = ctrl[1];

          if (!valFormManager) {
            return;
          }

          //listen for form validation changes
          valFormManager.onValidationStatusChanged(function (evt, args) {
            const { tab, groups } = scope.$eval(attrs.valGroupsInTab)

            if (!tab && !groups) {
              tab.hasError = false;
              return;
            }

            const form = args.form;

            const tabGroups = [];
            const tabGroupsIds = groups.filter(group => group.tabId === tab.id).map(group => group.id);

            for (const [key, value] of Object.entries(form)) {
              if (key.startsWith('groupForm')) {
                const matches = key.match(/\d+/);
                const groupId = matches && matches.length > 0 ? parseInt(matches[0]) : null;
                if (groupId && tabGroupsIds.indexOf(groupId) !== -1) {
                  tabGroups.push(value);
                }
              }
            }

            tab.hasError = tabGroups.filter(group => group.$invalid).length > 0;
          });

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
