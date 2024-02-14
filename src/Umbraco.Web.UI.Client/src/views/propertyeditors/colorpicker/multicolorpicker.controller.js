angular.module("umbraco").controller("Umbraco.PrevalueEditors.MultiColorPickerController",
  function ($scope, angularHelper, $element, eventsService) {

    const vm = this;

    vm.add = add;
    vm.addOnEnter = addOnEnter;
    vm.validateLabel = validateLabel;
    vm.remove = remove;
    vm.edit = edit;
    vm.cancel = cancel;

    vm.show = show;
    vm.hide = hide;
    vm.change = change;

    vm.labelEnabled = false;
    vm.editItem = null;

    // NOTE: We need to make each color an object, not just a string because you cannot 2-way bind to a primitive.
    const defaultColor = "000000";
    const defaultLabel = null;

    $scope.newColor = defaultColor;
    $scope.newLabel = defaultLabel;
    $scope.colorHasError = false;
    $scope.labelHasError = false;
    $scope.focusOnNew = false;

    $scope.options = {
      type: "color",
      color: defaultColor,
      allowEmpty: false,
      showAlpha: false
    };

    function hide() {
      // show the add button
      $element.find(".btn.add").show();
    }

    function show() {
      // hide the add button
      $element.find(".btn.add").hide();
    }

    function change(color) {
      angularHelper.safeApply($scope, function () {
        if (color) {
          $scope.newColor = color.toHexString().trimStart("#");
          $scope.colorHasError = !colorIsValid();
        }
      });
    }

    var evts = [];
    evts.push(eventsService.on("toggleValue", function (e, args) {
      if (args.inputId === "useLabel") {
        vm.labelEnabled = args.value;
      }
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
        if (Object.prototype.hasOwnProperty.call(oldValue, "value")) {
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
            sortOrder: oldValue.sortOrder,
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
    for (var ii = 0; ii < $scope.model.value.length; ii++) {
      var item = $scope.model.value[ii];
      item.label = Object.prototype.hasOwnProperty.call(item, "label") ? item.label : item.value;
    }

    function remove(item, evt) {

      evt.preventDefault();

      $scope.model.value = _.reject($scope.model.value, function (x) {
        return x.value === item.value && x.label === item.label;
      });

      setDirty();
    }

    function colorIsValid() {
      var colorExists = _.find($scope.model.value, function (item) {
        return item != vm.editItem && item.value.toUpperCase() === $scope.newColor.toUpperCase();
      });

      return colorExists ? false : true;
    }

    function getLabel() {
      var validLabel = $scope.newLabel !== null && typeof $scope.newLabel !== "undefined" && $scope.newLabel !== "" && $scope.newLabel.length && $scope.newLabel.length > 0;
      return validLabel ? $scope.newLabel : $scope.newColor;
    }

    function labelIsValid() {
      var label = getLabel();
      label = label.toUpperCase();

      var labelExists = _.find($scope.model.value, function (item) {
        return item != vm.editItem && item.label.toUpperCase() === label;
      });

      return labelExists ? false : true;
    }

    function validateLabel() {
      $scope.labelHasError = !labelIsValid();
    }

    function addOnEnter(evt) {
      if (evt.keyCode === 13) {
        add(evt);
      }
    }

    function add(evt) {
      evt.preventDefault();

      if ($scope.newColor) {

        $scope.colorHasError = !colorIsValid();
        $scope.labelHasError = !labelIsValid();

        if ($scope.labelHasError || $scope.colorHasError) {
          return;
        }

        var newLabel = getLabel();

        if (vm.editItem == null) {
          $scope.model.value.push({
            value: $scope.newColor,
            label: newLabel
          });
        } else {

          if (vm.editItem.value === vm.editItem.label && vm.editItem.value === newLabel) {
            vm.editItem.label = $scope.newColor;

          }
          else {
            vm.editItem.label = newLabel;
          }

          vm.editItem.value = $scope.newColor;

          vm.editItem = null;
        }

        $scope.newLabel = "";
        $scope.colorHasError = false;
        $scope.labelHasError = false;
        $scope.focusOnNew = true;
        setDirty();
        return;

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
      update: function () {
        setDirty();
      }
    };

  });
