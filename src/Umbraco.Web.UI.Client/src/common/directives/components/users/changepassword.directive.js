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

        var unsubscribe = [];

        function resetModel(isNew) {
            //the model config will contain an object, if it does not we'll create defaults
            //NOTE: We will not support doing the password regex on the client side because the regex on the server side
            //based on the membership provider cannot always be ported to js from .net directly.        
            /*
            {
                hasPassword: true/false,
                requiresQuestionAnswer: true/false,
                enableReset: true/false,
                enablePasswordRetrieval: true/false,
                minPasswordLength: 10
            }
            */

            vm.showReset = false;

            //set defaults if they are not available
            if (vm.config.disableToggle === undefined) {
                vm.config.disableToggle = false;
            }
            if (vm.config.hasPassword === undefined) {
                vm.config.hasPassword = false;
            }
            if (vm.config.enablePasswordRetrieval === undefined) {
                vm.config.enablePasswordRetrieval = true;
            }
            if (vm.config.requiresQuestionAnswer === undefined) {
                vm.config.requiresQuestionAnswer = false;
            }
            //don't enable reset if it is new - that doesn't make sense
            if (isNew === "true") {
                vm.config.enableReset = false;
            }
            else if (vm.config.enableReset === undefined) {
                vm.config.enableReset = true;
            }

            if (vm.config.minPasswordLength === undefined) {
                vm.config.minPasswordLength = 0;
            }

            //set the model defaults
            if (!angular.isObject(vm.passwordValues)) {
                //if it's not an object then just create a new one
                vm.passwordValues = {
                    newPassword: null,
                    oldPassword: null,
                    reset: null,
                    answer: null
                };
            }
            else {
                //just reset the values

                if (!isNew) {
                    //if it is new, then leave the generated pass displayed
                    vm.passwordValues.newPassword = null;
                    vm.passwordValues.oldPassword = null;
                }
                vm.passwordValues.reset = null;
                vm.passwordValues.answer = null;
            }

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
                //if there was a previously generated password displaying, clear it
                if (vm.changing && vm.passwordValues) {
                    vm.passwordValues.generatedPassword = null;
                }
                else if (!vm.changing) {
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
            //if there was a previously generated password displaying, clear it
            vm.passwordValues.generatedPassword = null;
            vm.passwordValues.confirm = null;
        };

        function cancelChange() {
            vm.changing = false;
            //set model to null
            vm.passwordValues = null;
        };

        function showOldPass() {
            return vm.config.hasPassword &&
                !vm.config.allowManuallyChangingPassword &&
                !vm.config.enablePasswordRetrieval && !vm.showReset;
        };

        // TODO: I don't think we need this or the cancel button, this can be up to the editor rendering this component
        function showCancelBtn() {
            return vm.config.disableToggle !== true && vm.config.hasPassword;
        };

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
