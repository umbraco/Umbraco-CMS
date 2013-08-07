/*
 jQuery UI Sortable plugin wrapper

 https://github.com/angular-ui/ui-sortable

 @param [ui-sortable] {object} Options to pass to $.fn.sortable() merged onto ui.config
*/
angular.module('ui.sortable', [])
  .value('uiSortableConfig', {})
  .directive('uiSortable', ['uiSortableConfig',
        function (uiSortableConfig) {
            return {
                require: '?ngModel',
                link: function (scope, element, attrs, ngModel) {

                    function combineCallbacks(first, second) {
                        if (second && (typeof second === "function")) {
                            return function (e, ui) {
                                first(e, ui);
                                second(e, ui);
                            };
                        }
                        return first;
                    }

                    var opts = {};

                    var callbacks = {
                        receive: null,
                        remove: null,
                        start: null,
                        stop: null,
                        update: null
                    };

                    angular.extend(opts, uiSortableConfig);

                    if (ngModel) {

                        ngModel.$render = function () {
                            element.sortable("refresh");
                        };

                        callbacks.start = function (e, ui) {
                            // Save position of dragged item
                            ui.item.sortable = { index: ui.item.index() };
                        };

                        callbacks.update = function (e, ui) {
                            // For some reason the reference to ngModel in stop() is wrong
                            ui.item.sortable.resort = ngModel;
                        };

                        callbacks.receive = function (e, ui) {
                            ui.item.sortable.relocate = true;
                            // added item to array into correct position and set up flag
                            ngModel.$modelValue.splice(ui.item.index(), 0, ui.item.sortable.moved);
                        };

                        callbacks.remove = function (e, ui) {
                            // copy data into item
                            if (ngModel.$modelValue.length === 1) {
                                ui.item.sortable.moved = ngModel.$modelValue.splice(0, 1)[0];
                            } else {
                                ui.item.sortable.moved = ngModel.$modelValue.splice(ui.item.sortable.index, 1)[0];
                            }
                        };

                        callbacks.stop = function (e, ui) {
                            // digest all prepared changes
                            if (ui.item.sortable.resort && !ui.item.sortable.relocate) {

                                // Fetch saved and current position of dropped element
                                var end, start;
                                start = ui.item.sortable.index;
                                end = ui.item.index();

                                // Reorder array and apply change to scope
                                ui.item.sortable.resort.$modelValue.splice(end, 0, ui.item.sortable.resort.$modelValue.splice(start, 1)[0]);

                            }
                            if (ui.item.sortable.resort || ui.item.sortable.relocate) {
                                scope.$apply();
                            }
                        };

                    }


                    scope.$watch(attrs.uiSortable, function (newVal, oldVal) {
                        angular.forEach(newVal, function (value, key) {

                            if (callbacks[key]) {
                                // wrap the callback
                                value = combineCallbacks(callbacks[key], value);
                            }

                            element.sortable('option', key, value);
                        });
                    }, true);

                    angular.forEach(callbacks, function (value, key) {

                        opts[key] = combineCallbacks(value, opts[key]);
                    });

                    // Create sortable

                    element.sortable(opts);
                }
            };
        }
  ]);