(function () {
    "use strict";
    
    function ScheduleContentController($scope, $timeout, localizationService, dateHelper, userService) {

        var vm = this;

        vm.datePickerSetup = datePickerSetup;
        vm.datePickerChange = datePickerChange;
        vm.datePickerShow = datePickerShow;
        vm.datePickerClose = datePickerClose;
        vm.clearPublishDate = clearPublishDate;
        vm.clearUnpublishDate = clearUnpublishDate;
        vm.dirtyVariantFilter = dirtyVariantFilter;
        vm.pristineVariantFilter = pristineVariantFilter;
        vm.changeSelection = changeSelection;

        vm.firstSelectedDates = {};
        vm.currentUser = null;

        function onInit() {

            vm.variants = $scope.model.variants;
            vm.hasPristineVariants = false;

            if(!$scope.model.title) {
                localizationService.localize("general_scheduledPublishing").then(function(value){
                    $scope.model.title = value;
                });
            }

            // Check for variants: if a node is invariant it will still have the default language in variants
            // so we have to check for length > 1
            if (vm.variants.length > 1) {

                _.each(vm.variants,
                    function (variant) {
                        variant.compositeId = variant.language.culture + "_" + (variant.segment ? variant.segment : "");
                        variant.htmlId = "_content_variant_" + variant.compositeId;
    
                        //check for pristine variants
                        if (!vm.hasPristineVariants) {
                            vm.hasPristineVariants = pristineVariantFilter(variant);
                        }
                    });

                //now sort it so that the current one is at the top
                vm.variants = _.sortBy(vm.variants, function (v) {
                    return v.active ? 0 : 1;
                });

                var active = _.find(vm.variants, function (v) {
                    return v.active;
                });

                if (active) {
                    //ensure that the current one is selected
                    active.schedule = true;
                    active.save = true;
                }

                $scope.model.disableSubmitButton = !canSchedule();
            
            }

            // get current backoffice user and format dates
            userService.getCurrentUser().then(function (currentUser) {

                vm.currentUser = currentUser;

                angular.forEach(vm.variants, function(variant) {

                    // prevent selecting publish/unpublish date before today
                    var now = new Date();
                    var nowFormatted = moment(now).format("YYYY-MM-DD HH:mm");

                    var datePickerConfig = {
                        enableTime: true,
                        dateFormat: "Y-m-d H:i",
                        time_24hr: true,
                        minDate: nowFormatted,
                        defaultDate: nowFormatted
                    };

                    variant.datePickerConfig = datePickerConfig;
                    
                    // format all dates to local
                    if(variant.releaseDate || variant.removeDate) {
                        formatDatesToLocal(variant);
                    }
                });

            });

        }

        /**
         * Callback when date is set up
         * @param {any} variant
         * @param {any} type publish or unpublish 
         * @param {any} datePickerInstance The date picker instance
         */
        function datePickerSetup(variant, type, datePickerInstance) {
            // store a date picker instance for publish and unpublish picker
            // so we can change the settings independently.
            if (type === 'publish') {
                variant.releaseDatePickerInstance = datePickerInstance;
            } else if (type === 'unpublish') {
                variant.removeDatePickerInstance = datePickerInstance;
            }
        };

        /**
         * Callback when date picker date changes
         * @param {any} variant 
         * @param {any} dateStr Date string from the date picker
         * @param {any} type publish or unpublish
         */
        function datePickerChange(variant, dateStr, type) {
            if (type === 'publish') {
                setPublishDate(variant, dateStr);
            } else if (type === 'unpublish') {
                setUnpublishDate(variant, dateStr);
            }
        }

        /**
         * Add flag when a date picker opens is we can prevent the overlay from closing
         * @param {any} variant 
         * @param {any} type publish or unpublish
         */
        function datePickerShow(variant, type) {
            if (type === 'publish') {
                variant.releaseDatePickerOpen = true;
            } else if (type === 'unpublish') {
                variant.removeDatePickerOpen = true;
            }
            checkForBackdropClick();
        }

        /**
         * Remove flag when a date picker closes so the overlay can be closed again
         * @param {any} variant 
         * @param {any} type publish or unpublish
         */
        function datePickerClose(variant, type) {
            $timeout(function(){
                if (type === 'publish') {
                    variant.releaseDatePickerOpen = false;
                } else if (type === 'unpublish') {
                    variant.removeDatePickerOpen = false;
                }
                checkForBackdropClick();
            }, 200);
        }

        /**
         * Prevent the overlay from closing if any date pickers are open
         */
        function checkForBackdropClick() {

            var open = _.find(vm.variants, function (variant) {
                return variant.releaseDatePickerOpen || variant.removeDatePickerOpen;
            });

            if(open) {
                $scope.model.disableBackdropClick = true;
            } else {
                $scope.model.disableBackdropClick = false;
            }
        }

        /**
         * Sets the selected publish date
         * @param {any} variant 
         * @param {any} date The selected date
         */
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

            // make sure the unpublish date can't be before the publish date
            variant.removeDatePickerInstance.set("minDate", moment(variant.releaseDate).format("YYYY-MM-DD HH:mm"));

        }

        /**
         * Sets the selected unpublish date
         * @param {any} variant 
         * @param {any} date The selected date
         */
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

            // make sure the publish date can't be after the publish date
            variant.releaseDatePickerInstance.set("maxDate", moment(variant.removeDate).format("YYYY-MM-DD HH:mm"));

        }

        /**
         * Clears the publish date
         * @param {any} variant 
         */
        function clearPublishDate(variant) {
            if(variant && variant.releaseDate) {
                variant.releaseDate = null;
                // we don't have a publish date anymore so we can clear the min date for unpublish
                var now = new Date();
                var nowFormatted = moment(now).format("YYYY-MM-DD HH:mm");
                variant.removeDatePickerInstance.set("minDate", nowFormatted);
            }
        }

        /**
         * Clears the unpublish date
         * @param {any} variant 
         */
        function clearUnpublishDate(variant) {
            if(variant && variant.removeDate) {
                variant.removeDate = null;
                // we don't have a unpublish date anymore so we can clear the max date for publish
                variant.releaseDatePickerInstance.set("maxDate", null);
            }
        }

        /**
         * Formates the selected dates to fit the user culture
         * @param {any} variant 
         */
        function formatDatesToLocal(variant) {
            if(variant && variant.releaseDate) {
                variant.releaseDateFormatted = dateHelper.getLocalDate(variant.releaseDate, vm.currentUser.locale, "MMM Do YYYY, HH:mm");
            }
            if(variant && variant.removeDate) {
                variant.removeDateFormatted = dateHelper.getLocalDate(variant.removeDate, vm.currentUser.locale, "MMM Do YYYY, HH:mm");
            }
        }
        
        /**
         * Called when new variants are selected or deselected
         * @param {any} variant 
         */
        function changeSelection(variant) {
            $scope.model.disableSubmitButton = !canSchedule();
            //need to set the Save state to true if publish is true
            variant.save = variant.schedule;
        }

        function dirtyVariantFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * the active one
            // * it's editor is in a $dirty state
            // * it has pending saves
            // * it is unpublished
            // * it is in NotCreated state
            return (variant.active || variant.isDirty || variant.state === "Draft" || variant.state === "PublishedPendingChanges" || variant.state === "NotCreated");
        }

        function pristineVariantFilter(variant) {
            return !(dirtyVariantFilter(variant));
        }

        /** Returns true if publishing is possible based on if there are un-published mandatory languages */
        function canSchedule() {
            var selected = [];
            for (var i = 0; i < vm.variants.length; i++) {
                var variant = vm.variants[i];

                //if this variant will show up in the publish-able list
                var publishable = dirtyVariantFilter(variant);

                if ((variant.language.isMandatory && (variant.state === "NotCreated" || variant.state === "Draft"))
                    && (!publishable || !variant.schedule)) {
                    //if a mandatory variant isn't published and it's not publishable or not selected to be published
                    //then we cannot publish anything

                    //TODO: Show a message when this occurs
                    return false;
                }

                if (variant.schedule) {
                    selected.push(variant.schedule);
                }
            }
            return selected.length > 0;
        }

        onInit();

        //when this dialog is closed, clean up
        $scope.$on('$destroy', function () {
            for (var i = 0; i < vm.variants.length; i++) {
                vm.variants[i].save = false;
                vm.variants[i].schedule = false;
                // remove properties only needed for this dialog
                delete vm.variants[i].releaseDateFormatted;
                delete vm.variants[i].removeDateFormatted;
                delete vm.variants[i].datePickerConfig;
                delete vm.variants[i].releaseDatePickerInstance;
                delete vm.variants[i].removeDatePickerInstance;
                delete vm.variants[i].releaseDatePickerOpen;
                delete vm.variants[i].removeDatePickerOpen;
            } 
        });

    }

    angular.module("umbraco").controller("Umbraco.Overlays.ScheduleContentController", ScheduleContentController);
    
})();
