(function () {
    'use strict';

    angular
        .module('umbraco.directives')        
        .component('umbPasswordTip', {
            controller: UmbPasswordTipController,
            controllerAs: 'vm',
            template: '<span class="help-inline" style="display: block;" ng-if="vm.passwordTip" ng-bind-html="vm.passwordTip">{{vm.passwordTip}}</span>'
        });

    function UmbPasswordTipController($scope, $attrs, localizationService) {
        const minPwdLength = Umbraco.Sys.ServerVariables.umbracoSettings.minimumPasswordLength;
        const minPwdNonAlphaNum = Umbraco.Sys.ServerVariables.umbracoSettings.minimumPasswordNonAlphaNum;

        let passwordNonAlphaTip;
        let passwordWatcher;

        // because the password field can have min-length, we can't watch the raw value, since it
        // won't exist until it meets the minimum length, which means the password tip won't be 
        // visible until the password meets the min length, at which point the tip is redundant
        const element = document.querySelector($attrs.bindTo);
        if (element) {
            passwordWatcher = $scope.$watch(
                () => element.value,             
                newVal => updatePasswordTip(newVal.length));
        }

        if (minPwdNonAlphaNum > 0) {
            localizationService.localize('user_newPasswordFormatNonAlphaTip', [minPwdNonAlphaNum]).then(data => {
                passwordNonAlphaTip = data;
                updatePasswordTip(0);
            });
        } else {
            passwordNonAlphaTip = '';
            updatePasswordTip(0);
        }

        const updatePasswordTip = passwordLength => {
            const remainingLength = minPwdLength - passwordLength;
            if (remainingLength > 0) {
                localizationService.localize('user_newPasswordFormatLengthTip', [remainingLength]).then(data => {
                    this.passwordTip = data;
                    if (passwordNonAlphaTip) {
                        this.passwordTip += `<br/>${passwordNonAlphaTip}`;
                    }
                });
            } else {
                this.passwordTip = passwordNonAlphaTip;
            }
        }

        $scope.$on('$destroy', () => passwordWatcher());
    }

})();