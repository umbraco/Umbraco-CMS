angular.module("umbraco").controller("Umbraco.PrevalueEditors.MultiColorPickerController",
    function ($scope, angularHelper, $element, eventsService) {

        var vm = this;

        vm.add = add;
        vm.remove = remove;
        vm.edit = edit;
        vm.cancel = cancel;

        vm.show = show;
        vm.hide = hide;
        vm.change = change;

        vm.labelEnabled = false;
        vm.editItem = null;

        //NOTE: We need to make each color an object, not just a string because you cannot 2-way bind to a primitive.
        var defaultColor = "000000";
        var defaultLabel = null;

        $scope.newColor = defaultColor;
        $scope.newLabel = defaultLabel;
        $scope.hasError = false;
        $scope.focusOnNew = false;

        $scope.options = {
            type: "color",
            color: defaultColor,
            allowEmpty: false,
            showAlpha: false
        };

        function hide(color) {
            // show the add button
            $element.find(".btn.add").show();
        }

        function show(color) {
            // hide the add button
            $element.find(".btn.add").hide();
        }

        function change(color) {
            angularHelper.safeApply($scope, function () {
                if (color) {
                    $scope.newColor = color.toHexString().trimStart("#");
                }
            });
        }
        var evts = [];
        evts.push(eventsService.on("toggleValue", function (e, args) {
            vm.labelEnabled = args.value;
        }));
        $scope.$on('$destroy', function () {
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }
        });
        if (!Utilities.isArray($scope.model.value)) {
            //make an array from the dictionary
            var items = [];
            for (var i in $scope.model.value) {
                var oldValue = $scope.model.value[i];
                if (oldValue.hasOwnProperty("value")) {
                    items.push({
                        value: oldValue.value,
                        label: oldValue.label,
                        sortOrder: oldValue.sortOrder,
                        id: i
                    });
                } else {
                    items.push({
                        value: oldValue,
                        label: oldValue,
                        sortOrder: sortOrder,
                        id: i
                    });
                }
            }

            //ensure the items are sorted by the provided sort order
            items.sort(function (a, b) { return (a.sortOrder > b.sortOrder) ? 1 : ((b.sortOrder > a.sortOrder) ? -1 : 0); });

            //now make the editor model the array
            $scope.model.value = items;
        }

        // ensure labels
        for (var i = 0; i < $scope.model.value.length; i++) {
            var item = $scope.model.value[i];
            item.label = item.hasOwnProperty("label") ? item.label : item.value;
        }

        function validLabel(label) {
            return label !== null && typeof label !== "undefined" && label !== "" && label.length && label.length > 0;
        }

        function remove(item, evt) {

            evt.preventDefault();

            $scope.model.value = _.reject($scope.model.value, function (x) {
                return x.value === item.value && x.label === item.label;
            });

            setDirty();
        }

        function add(evt) {
            evt.preventDefault();

            if ($scope.newColor) {
                var newLabel = validLabel($scope.newLabel) ? $scope.newLabel : $scope.newColor;
                var exists = _.find($scope.model.value, function (item) {
                    return item != vm.editItem && (item.value.toUpperCase() === $scope.newColor.toUpperCase() || item.label.toUpperCase() === newLabel.toUpperCase());
                });
                if (!exists) {
                    if (vm.editItem == null) {
                        $scope.model.value.push({
                            value: $scope.newColor,
                            label: newLabel
                        });
                    } else {
                        vm.editItem.value = $scope.newColor;
                        vm.editItem.label = newLabel;
                        vm.editItem = null;
                    }
                    
                    $scope.newLabel = "";
                    $scope.hasError = false;
                    $scope.focusOnNew = true;
                    setDirty();
                    return;
                }

                // there was an error, do the highlight (will be set back by the directive)
                $scope.hasError = true;
            }
        }

        function edit(item, evt) {
            evt.preventDefault();

            vm.editItem = item;

            $scope.newColor = item.value;
            $scope.newLabel = item.label;
        }

        function cancel(evt) {
            evt.preventDefault();

            vm.editItem = null;
            $scope.newColor = defaultColor;
            $scope.newLabel = defaultLabel;
        }

        function setDirty() {
            if (vm.modelValueForm) {
                vm.modelValueForm.selectedColor.$setDirty();
            }
        }

        $scope.sortableOptions = {
            axis: 'y',
            containment: 'parent',
            cursor: 'move',
            //handle: ".handle, .thumbnail",
            items: '> div.control-group',
            tolerance: 'pointer',
            update: function (e, ui) {
                setDirty();
            }
        };

    });
