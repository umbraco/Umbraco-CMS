(function () {
    'use strict';

    function onOutsideClickDirective($timeout, angularHelper){

        return function (scope, element, attrs) {

            var eventBindings = [];

            function oneTimeClick(event) {
                var el = event.target.nodeName;

                //ignore link and button clicks
                var els = ["INPUT", "A", "BUTTON"];
                if (els.indexOf(el) >= 0) { return; }

                // ignore clicks on new overlay
                var parents = $(event.target).parents("a,button,.umb-overlay,.umb-tour");
                if (parents.length > 0) {
                    return;
                }

                // ignore clicks on dialog from old dialog service
                var oldDialog = $(event.target).parents("#old-dialog-service");
                if (oldDialog.length === 1) {
                    return;
                }

                // ignore clicks in tinyMCE dropdown(floatpanel)
                var floatpanel = $(event.target).closest(".mce-floatpanel");
                if (floatpanel.length === 1) {
                    return;
                }

                // ignore clicks in flatpickr datepicker
                var flatpickr = $(event.target).closest(".flatpickr-calendar");
                if (flatpickr.length === 1) {
                    return;
                }

                //ignore clicks inside this element
                if ($(element).has($(event.target)).length > 0) {
                    return;
                }

                // please to not use angularHelper.safeApply here, it won't work
                scope.$evalAsync(attrs.onOutsideClick);
            }


            $timeout(function () {

                if ("bindClickOn" in attrs) {

                    eventBindings.push(scope.$watch(function () {
                        return attrs.bindClickOn;
                    }, function (newValue) {
                        if (newValue === "true") {
                            $(document).on("click", oneTimeClick);
                        } else {
                            $(document).off("click", oneTimeClick);
                        }
                    }));

                } else {
                    $(document).on("click", oneTimeClick);
                }

                scope.$on("$destroy", function () {
                    $(document).off("click", oneTimeClick);

                    // unbind watchers
                    for (var e in eventBindings) {
                        eventBindings[e]();
                    }

                });
            }); // Temp removal of 1 sec timeout to prevent bug where overlay does not open. We need to find a better solution.

        };

    }

    angular.module('umbraco.directives').directive('onOutsideClick', onOutsideClickDirective);

})();
