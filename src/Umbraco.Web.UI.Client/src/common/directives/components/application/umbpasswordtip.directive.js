(function () {
    'use strict';

    angular
        .module('umbraco.directives')        
        .component('umbPasswordTip', {
            controller: UmbPasswordTipController,
            controllerAs: 'vm',
            template: '<span class="help-inline" style="display: block;" ng-if="vm.passwordTip" ng-bind-html="vm.passwordTip">{{vm.passwordTip}}</span>',
            bindings: {
                minPwdLength: "<",
                minPwdNonAlphaNum: "<"
            }
        });

    function UmbPasswordTipController($scope, $attrs, localizationService) {

        let defaultMinPwdLength = Umbraco.Sys.ServerVariables.umbracoSettings.minimumPasswordLength;
        let defaultMinPwdNonAlphaNum = Umbraco.Sys.ServerVariables.umbracoSettings.minimumPasswordNonAlphaNum;

        var vm = this;
        vm.$onInit = onInit;

        function onInit() {
            if (vm.minPwdLength === undefined) {
                vm.minPwdLength = defaultMinPwdLength;
            }

            if (vm.minPwdNonAlphaNum === undefined) {
                vm.minPwdNonAlphaNum = defaultMinPwdNonAlphaNum;
            }

            // because the password field can have min-length, we can't watch the raw value, since it
            // won't exist until it meets the minimum length, which means the password tip won't be 
            // visible until the password meets the min length, at which point the tip is redundant
            const element = document.querySelector($attrs.bindTo);
            if (element) {
                vm.passwordWatcher = $scope.$watch(
                    () => element.value,
                    newVal => updatePasswordTip(newVal.length));
            }

            if (vm.minPwdNonAlphaNum > 0) {
                localizationService.localize('user_newPasswordFormatNonAlphaTip', [vm.minPwdNonAlphaNum]).then(data => {
                    vm.passwordNonAlphaTip = data;
                    updatePasswordTip(0);
                });
            } else {
                vm.passwordNonAlphaTip = '';
                updatePasswordTip(0);
            }
        }

        const updatePasswordTip = passwordLength => {
            const remainingLength = vm.minPwdLength - passwordLength;
            if (remainingLength > 0) {
                localizationService.localize('user_newPasswordFormatLengthTip', [remainingLength]).then(data => {
                    vm.passwordTip = data;
                    if (vm.passwordNonAlphaTip) {
                        vm.passwordTip += `<br/>${vm.passwordNonAlphaTip}`;
                    }
                });
            } else {
                vm.passwordTip = vm.passwordNonAlphaTip;
            }
        }

        $scope.$on('$destroy', () => vm.passwordWatcher());
    }

})();
