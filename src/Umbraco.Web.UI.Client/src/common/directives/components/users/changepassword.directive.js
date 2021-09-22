(function () {
    'use strict';

    function ChangePasswordController($scope) {

        var vm = this;

        vm.$onInit = onInit;
        vm.$onDestroy = onDestroy;
        vm.doChange = doChange;
        vm.cancelChange = cancelChange;
        vm.showOldPass = showOldPass;
        vm.showCancelBtn = showCancelBtn;
        vm.newPasswordKeyUp = newPasswordKeyUp;

        var unsubscribe = [];

        function resetModel(isNew) {
            //the model config will contain an object, if it does not we'll create defaults
            /*
            {
                hasPassword: true/false,
                minPasswordLength: 10
            }
            */

            //set defaults if they are not available
            if (vm.config.disableToggle === undefined) {
                vm.config.disableToggle = false;
            }
            if (vm.config.hasPassword === undefined) {
                vm.config.hasPassword = false;
            }
            if (vm.config.minPasswordLength === undefined) {
                vm.config.minPasswordLength = 0;
            }

            // Check non-alpha pwd settings for tooltip display
            if (vm.config.minNonAlphaNumericChars === undefined) {
                vm.config.minNonAlphaNumericChars = 0;
            }

            //set the model defaults
            if (!Utilities.isObject(vm.passwordValues)) {
                //if it's not an object then just create a new one
                vm.passwordValues = {
                    newPassword: null,
                    oldPassword: null
                };
            }
            else {
                //just reset the values

                if (!isNew) {
                    //if it is new, then leave the generated pass displayed
                    vm.passwordValues.newPassword = null;
                    vm.passwordValues.oldPassword = null;
                }
            }

            // set initial value for new password value
            vm.passwordVal = vm.passwordValues.newPassword;

            //the value to compare to match passwords
            if (!isNew) {
                vm.passwordValues.confirm = "";
            }
            else if (vm.passwordValues.newPassword && vm.passwordValues.newPassword.length > 0) {
                //if it is new and a new password has been set, then set the confirm password too
                vm.passwordValues.confirm = vm.passwordValues.newPassword;
            }

        }

        //when the scope is destroyed we need to unsubscribe
        function onDestroy() {
            for (var u in unsubscribe) {
                unsubscribe[u]();
            }
        }

        function onInit() {
            //listen for the saved event, when that occurs we'll
            //change to changing = false;
            unsubscribe.push($scope.$on("formSubmitted", function () {
                if (vm.config.disableToggle === false) {
                    vm.changing = false;
                }
            }));

            unsubscribe.push($scope.$on("formSubmitting", function () {
                if (!vm.changing) {
                    //we are not changing, so the model needs to be null
                    vm.passwordValues = null;
                }
            }));

            resetModel(vm.isNew);

            //if there is no password saved for this entity , it must be new so we do not allow toggling of the change password, it is always there
            //with validators turned on.
            vm.changing = vm.config.disableToggle === true || !vm.config.hasPassword;

            //we're not currently changing so set the model to null
            if (!vm.changing) {
                vm.passwordValues = null;
            }
        }

        function doChange() {
            resetModel();
            vm.changing = true;
            vm.passwordValues.confirm = null;
        };

        function cancelChange() {
            vm.changing = false;
            //set model to null
            vm.passwordValues = null;
        };

        function showOldPass() {
            return vm.config.hasPassword &&
                !vm.config.allowManuallyChangingPassword;
        };

        // TODO: I don't think we need this or the cancel button, this can be up to the editor rendering this component
        function showCancelBtn() {
            return vm.config.disableToggle !== true && vm.config.hasPassword;
        };

        function newPasswordKeyUp(event) {
            vm.passwordVal = event.target.value;
        }
    }

    var component = {
        templateUrl: 'views/components/users/change-password.html',
        controller: ChangePasswordController,
        controllerAs: 'vm',
        bindings: {
            isNew: "<",
            passwordValues: "=", //TODO: Do we need bi-directional vals?
            config: "=" //TODO: Do we need bi-directional vals?
            //TODO: Do we need callbacks?
        }
    };

    angular.module('umbraco.directives').component('changePassword', component);


})();
