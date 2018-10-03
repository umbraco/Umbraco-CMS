(function () {
    "use strict";
    
    function ScheduleContentController($scope, localizationService, dateHelper, userService) {

        var vm = this;

        vm.datePickerChange = datePickerChange;
        vm.clearPublishDate = clearPublishDate;
        vm.clearUnpublishDate = clearUnpublishDate;

        vm.currentUser = null;
        vm.datePickerConfig = {
            pickDate: true,
            pickTime: true,
            useSeconds: false,
            format: "YYYY-MM-DD HH:mm",
            icons: {
                time: "icon-time",
                date: "icon-calendar",
                up: "icon-chevron-up",
                down: "icon-chevron-down"
            }
        };

        function onInit() {

            vm.variants = $scope.model.variants;

            if(!$scope.model.title) {
                localizationService.localize("general_scheduledPublishing").then(function(value){
                    $scope.model.title = value;
                });
            }

            // get current backoffice user and format dates
            userService.getCurrentUser().then(function (currentUser) {

                vm.currentUser = currentUser;

                // format all dates to local
                angular.forEach(vm.variants, function(variant) {
                    if(variant.releaseDate || variant.removeDate) {
                        formatDatesToLocal(variant);
                    }
                });

            });

        }

        function datePickerChange(variant, event, type) {
            if (type === 'publish') {
                setPublishDate(variant, event.date.format("YYYY-MM-DD HH:mm"));
            } else if (type === 'unpublish') {
                setUnpublishDate(variant, event.date.format("YYYY-MM-DD HH:mm"));
            }
        }

        function setPublishDate(variant, date) {

            if (!date) {
                return;
            }

            //The date being passed in here is the user's local date/time that they have selected
            //we need to convert this date back to the server date on the model.
            var serverTime = dateHelper.convertToServerStringTime(moment(date), Umbraco.Sys.ServerVariables.application.serverTimeOffset);

            // update publish value
            variant.releaseDate = serverTime;

            // make sure dates are formatted to the user's locale
            formatDatesToLocal(variant);

        }

        function setUnpublishDate(variant, date) {

            if (!date) {
                return;
            }

            //The date being passed in here is the user's local date/time that they have selected
            //we need to convert this date back to the server date on the model.
            var serverTime = dateHelper.convertToServerStringTime(moment(date), Umbraco.Sys.ServerVariables.application.serverTimeOffset);

            // update publish value
            variant.removeDate = serverTime;

            // make sure dates are formatted to the user's locale
            formatDatesToLocal(variant);

        }

        function clearPublishDate(variant) {
            if(variant && variant.releaseDate) {
                variant.releaseDate = null;
            }
        }

        function clearUnpublishDate(variant) {
            if(variant && variant.removeDate) {
                variant.removeDate = null;
            }
        }

        function formatDatesToLocal(variant) {

            if(variant && variant.releaseDate) {
                variant.releaseDateFormatted = dateHelper.getLocalDate(variant.releaseDate, vm.currentUser.locale, "YYYY-MM-DD HH:mm");
            }

            if(variant && variant.removeDate) {
                variant.removeDateFormatted = dateHelper.getLocalDate(variant.removeDate, vm.currentUser.locale, "YYYY-MM-DD HH:mm");
            }

        }

        onInit();

        //when this dialog is closed, reset all 'save' flags
        $scope.$on('$destroy', function () {
            for (var i = 0; i < vm.variants.length; i++) {
                vm.variants[i].save = false;
            }
        });

    }

    angular.module("umbraco").controller("Umbraco.Overlays.ScheduleContentController", ScheduleContentController);
    
})();
