(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbPasswordTip', {
            controller: UmbPasswordTipController,
            controllerAs: 'vm',
            template:
                '<span class="help-inline" style="display: block;" ng-if="vm.passwordTip" ng-bind-html="vm.passwordTip">{{vm.passwordTip}}</span>',
            bindings: {
                passwordVal: "<",
                minPwdLength: "<",
                minPwdNonAlphaNum: "<"
            }
        });

    function UmbPasswordTipController(localizationService) {

        let defaultMinPwdLength = Umbraco.Sys.ServerVariables.umbracoSettings.minimumPasswordLength;
        let defaultMinPwdNonAlphaNum = Umbraco.Sys.ServerVariables.umbracoSettings.minimumPasswordNonAlphaNum;

        var vm = this;
        vm.$onInit = onInit;
        vm.$onChanges = onChanges;

        function onInit() {
            if (vm.minPwdLength === undefined) {
                vm.minPwdLength = defaultMinPwdLength;
            }

            if (vm.minPwdNonAlphaNum === undefined) {
                vm.minPwdNonAlphaNum = defaultMinPwdNonAlphaNum;
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

        function onChanges(simpleChanges) {
            if (simpleChanges.passwordVal) {
                if (simpleChanges.passwordVal.currentValue) {
                    updatePasswordTip(simpleChanges.passwordVal.currentValue.length);
                } else {
                    updatePasswordTip(0);
                }
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
    }
})();
