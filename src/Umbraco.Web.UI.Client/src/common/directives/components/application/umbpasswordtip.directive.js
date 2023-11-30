(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbPasswordTip', {
            controller: UmbPasswordTipController,
            controllerAs: 'vm',
            template:
                '<span class="help-inline" style="display: block;" ng-if="vm.passwordTip" ng-bind-html="vm.passwordTip"></span>',
            bindings: {
                passwordVal: "<",
                minPwdLength: "<",
                minPwdNonAlphaNum: "<",
                minPwdDigit: "<",
                minPwdUppercase: "<",
                minPwdLowercase: "<"
            }
        });

    function UmbPasswordTipController(localizationService) {

        let defaultMinPwdLength = Umbraco.Sys.ServerVariables.umbracoSettings.minimumPasswordLength;
        let defaultMinPwdNonAlphaNum = Umbraco.Sys.ServerVariables.umbracoSettings.minimumPasswordNonAlphaNum;
        let defaultMinPwdDigit = Umbraco.Sys.ServerVariables.umbracoSettings.minimumPasswordDigit;
        let defaultMinPwdUppercase = Umbraco.Sys.ServerVariables.umbracoSettings.minimumPasswordUppercase;
        let defaultMinPwdLowercase = Umbraco.Sys.ServerVariables.umbracoSettings.minimumPasswordLowercase;

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

            if (vm.minPwdDigit === undefined) {
                vm.minPwdDigit = defaultMinPwdDigit;
            }

            if (vm.minPwdDigit === undefined) {
                vm.minPwdDigit = defaultMinPwdDigit;
            }

            if (vm.minPwdUppercase === undefined) {
                vm.minPwdUppercase = defaultMinPwdUppercase;
            }

            if (vm.minPwdLowercase === undefined) {
              vm.minPwdLowercase = defaultMinPwdLowercase;
          }

          vm.passwordLength = 0;
          vm.passwordNonAlphaTip = '';
          vm.passwordDigitTip = '';
          vm.passwordUppercaseTip = '';
          vm.passwordLowercaseTip = '';
          vm.passwordTip = '';

          vm.tips = []
          vm.tipStrs = ['NonAlpha', 'Digit', 'Uppercase', 'Lowercase']
          vm.mins = [vm.minPwdNonAlphaNum, vm.minPwdDigit, vm.minPwdUppercase, vm.minPwdLowercase]


          for (var i = 0; i < vm.tipStrs.length; i++) {
            setUpTip(vm.tipStrs[i], vm.mins[i]);
            }

            /*
            if (vm.minPwdNonAlphaNum > 0) {
                localizationService.localize('user_newPasswordFormatNonAlphaTip', [tip]).then(data => {
                    vm.passwordNonAlphaTip = data;
                    updatePasswordTip(vm.passwordLength);
                });
            } else {
                vm.passwordNonAlphaTip = '';
                updatePasswordTip(vm.passwordLength);
            }
            */
        }

        // Need to use minimum here
        function setUpTip(str, min) {
          if (min > 0) {
            localizationService.localize(`user_newPasswordFormat${str}Tip`, [min]).then(data => {
                    vm.tips.push(data);
                    updatePasswordTip(vm.passwordLength);
                });
            } else {
                vm.tips.push('')
                updatePasswordTip(vm.passwordLength);
            }
        }

        function onChanges(simpleChanges) {

            if (simpleChanges.passwordVal) {
                vm.passwordLength = simpleChanges.passwordVal.currentValue ? simpleChanges.passwordVal.currentValue.length : 0;

                updatePasswordTip(vm.passwordLength);
            }
        }

        const updatePasswordTip = passwordLength => {

            const remainingLength = vm.minPwdLength - passwordLength;
            
            if (remainingLength > 0) {
                localizationService.localize('user_newPasswordFormatLengthTip', [remainingLength]).then(data => {
                  vm.passwordTip = data;
                  vm.passwordTip += `<br/>`
                  vm.tips.forEach((tip) => {
                    if (tip != '') {
                      vm.passwordTip += `${tip}<br/>`
                    }
                  });
                });
            } else {
              vm.passwordTip = '';
              vm.tips.forEach((tip) => {
                if (tip != '') {
                  vm.passwordTip += `${tip}<br/>`
                }
              });
            }
        }
    }
})();
