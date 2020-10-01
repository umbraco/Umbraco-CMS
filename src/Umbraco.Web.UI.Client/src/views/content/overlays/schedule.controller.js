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
        vm.changeSelection = changeSelection;

        vm.firstSelectedDates = {};
        vm.currentUser = null;

        //used to track the original values so if the user doesn't save the schedule and they close the dialog we reset the dates back to what they were.
        var origDates = [];

        function onInit() {

            vm.variants = $scope.model.variants;
            vm.displayVariants = vm.variants.slice(0);// shallow copy, we dont want to share the array-object(because we will be performing a sort method) but each entry should be shared (because we need validation and notifications).

            if(!$scope.model.title) {
                localizationService.localize("general_scheduledPublishing").then(value => {
                    $scope.model.title = value;
                });
            }

            vm.variants.forEach(variant => {
                origDates.push({
                    releaseDate: variant.releaseDate,
                    expireDate: variant.expireDate
                });

                variant.isMandatory = isMandatoryFilter(variant);
            });

            // Check for variants: if a node is invariant it will still have the default language in variants
            // so we have to check for length > 1
            if (vm.variants.length > 1) {

                vm.displayVariants.sort((a, b) => {
                    if (a.language && b.language) {
                        if (a.language.name < b.language.name) {
                            return -1;
                        }
                        if (a.language.name > b.language.name) {
                            return 1;
                        }
                    }
                    if (a.segment && b.segment) {
                        if (a.segment < b.segment) {
                            return -1;
                        }
                        if (a.segment > b.segment) {
                            return 1;
                        }
                    }
                    return 0;
                });

                vm.variants.forEach(v => {
                    if (v.active) {
                        v.save = true;
                    }
                });
                
                $scope.model.disableSubmitButton = !canSchedule();            
            }

            // get current backoffice user and format dates
            userService.getCurrentUser().then(currentUser => {

                vm.currentUser = currentUser;

                vm.variants.forEach(variant => {

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
                    if(variant.releaseDate || variant.expireDate) {
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
                variant.expireDatePickerInstance = datePickerInstance;
            }
            $scope.model.disableSubmitButton = !canSchedule();
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
            $scope.model.disableSubmitButton = !canSchedule();
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
                variant.expireDatePickerOpen = true;
            }
            checkForBackdropClick();
            $scope.model.disableSubmitButton = !canSchedule();
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
                    variant.expireDatePickerOpen = false;
                }
                checkForBackdropClick();
                $scope.model.disableSubmitButton = !canSchedule();
            }, 200);

        }

        /**
         * Prevent the overlay from closing if any date pickers are open
         */
        function checkForBackdropClick() {

            var open = vm.variants.find(variant => variant.releaseDatePickerOpen || variant.expireDatePickerOpen);

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
            variant.expireDatePickerInstance.set("minDate", moment(variant.releaseDate).format("YYYY-MM-DD HH:mm"));

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
            variant.expireDate = serverTime;

            // make sure dates are formatted to the user's locale
            formatDatesToLocal(variant);

            // make sure the publish date can't be after the publish date
            variant.releaseDatePickerInstance.set("maxDate", moment(variant.expireDate).format("YYYY-MM-DD HH:mm"));

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
                variant.expireDatePickerInstance.set("minDate", nowFormatted);
            }
            $scope.model.disableSubmitButton = !canSchedule();
        }

        /**
         * Clears the unpublish date
         * @param {any} variant 
         */
        function clearUnpublishDate(variant) {
            if(variant && variant.expireDate) {
                variant.expireDate = null;
                // we don't have a unpublish date anymore so we can clear the max date for publish
                variant.releaseDatePickerInstance.set("maxDate", null);
            }
            $scope.model.disableSubmitButton = !canSchedule();
        }

        /**
         * Formates the selected dates to fit the user culture
         * @param {any} variant 
         */
        function formatDatesToLocal(variant) {
            if(variant && variant.releaseDate) {
                variant.releaseDateFormatted = dateHelper.getLocalDate(variant.releaseDate, vm.currentUser.locale, "MMM Do YYYY, HH:mm");
            }
            if(variant && variant.expireDate) {
                variant.expireDateFormatted = dateHelper.getLocalDate(variant.expireDate, vm.currentUser.locale, "MMM Do YYYY, HH:mm");
            }
        }
        
        /**
         * Called when new variants are selected or deselected
         * @param {any} variant 
         */
        function changeSelection(variant) {
            $scope.model.disableSubmitButton = !canSchedule();
            //need to set the Save state to true if publish is true
            variant.save = variant.save;
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

        function isMandatoryFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * has a mandatory language
            // * without having a segment, segments cant be mandatory at current state of code.
            return (variant.language && variant.language.isMandatory === true && variant.segment == null);
        }

        /** Returns true if publishing is possible based on if there are un-published mandatory languages */
        function canSchedule() {

            // sched is enabled if
            //  1) when mandatory langs are not published AND all mandatory langs are selected AND all mandatory langs have a release date
            //  2) OR all mandatory langs are published
            //  3) OR all mandatory langs are are scheduled for publishing
            //  4) OR there has been a persisted schedule for a variant and it has now been changed

            var selectedWithDates = [];
            for (var i = 0; i < vm.variants.length; i++) {
                var variant = vm.variants[i];

                //if the sched dates for this variant have been removed then we must allow the schedule button to be used to save the changes
                var schedCleared = (origDates[i].releaseDate && origDates[i].releaseDate !== variant.releaseDate)
                    || (origDates[i].expireDate && origDates[i].expireDate !== variant.expireDate);
                if (schedCleared) {
                    return true;
                }

                var isMandatory = variant.segment == null && variant.language && variant.language.isMandatory;

                //if this variant will show up in the publish-able list
                var publishable = dirtyVariantFilter(variant);
                var published = !(variant.state === "NotCreated" || variant.state === "Draft");
                var isScheduledPublished = variant.releaseDate;

                if (isMandatory && !published && !isScheduledPublished && (!publishable || !variant.save)) {
                    //if a mandatory variant isn't published or scheduled published
                    //and it's not publishable or not selected to be published
                    //then we cannot continue

                    // TODO: Show a message when this occurs
                    return false;
                }

                if (variant.save && (variant.releaseDate || variant.expireDate)) {
                    selectedWithDates.push(variant.save);
                }
            }
            return selectedWithDates.length > 0;
        }
        
        onInit();

        //when this dialog is closed, clean up
        $scope.$on('$destroy', () => {
            vm.variants.forEach(variant => {
                variant.save = false;
                // remove properties only needed for this dialog
                delete variant.releaseDateFormatted;
                delete variant.expireDateFormatted;
                delete variant.datePickerConfig;
                delete variant.releaseDatePickerInstance;
                delete variant.expireDatePickerInstance;
                delete variant.releaseDatePickerOpen;
                delete variant.expireDatePickerOpen;
            }); 
        });
    }

    angular.module("umbraco").controller("Umbraco.Overlays.ScheduleContentController", ScheduleContentController);
    
})();
