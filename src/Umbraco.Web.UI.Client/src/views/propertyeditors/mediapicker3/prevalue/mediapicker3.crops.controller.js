angular.module("umbraco").controller("Umbraco.PropertyEditors.MediaPicker3.CropConfigurationController",
	function ($scope) {

        var unsubscribe = [];

	    if (!$scope.model.value) {
	        $scope.model.value = [];
	    }

        $scope.setFocus = false;

	    $scope.remove = function (crop, evt) {
	        evt.preventDefault();
	        const i = $scope.model.value.indexOf(crop);
            if (i > -1) {
                $scope.model.value.splice(i, 1);
            }
	    };

	    $scope.edit = function (crop, evt) {
            evt.preventDefault();
            crop.editMode = true;
	    };

        $scope.addNewCrop = function (evt) {
            evt.preventDefault();

            var crop = {};
            crop.editMode = true;

            $scope.model.value.push(crop);
            $scope.validate(crop);
        };

        $scope.setChanges = function (crop) {
            $scope.validate(crop);
            if(
                crop.hasWidthError !== true &&
                crop.hasHeightError !== true &&
                crop.hasAliasError !== true
            ) {
                crop.editMode = false;
                window.dispatchEvent(new Event('resize.umbImageGravity'));
            }
        };

        $scope.isEmpty = function (crop) {
            return !crop.label && !crop.alias && !crop.width && !crop.height;
        };

        $scope.useForAlias = function (crop) {
            if (crop.alias == null || crop.alias === "") {
                crop.alias = (crop.label || "").toCamelCase();
            }
        };

        $scope.validate = function (crop) {
            $scope.validateWidth(crop);
            $scope.validateHeight(crop);
            $scope.validateAlias(crop);
        };

        $scope.validateWidth = function (crop) {
            crop.hasWidthError = !(Utilities.isNumber(crop.width) && crop.width > 0);
        };

        $scope.validateHeight = function (crop) {
            crop.hasHeightError = !(Utilities.isNumber(crop.height) && crop.height > 0);
        };

        $scope.validateAlias = function (crop, $event) {
            var exists = $scope.model.value.find( x => crop !== x && crop.alias === x.alias);
            if (exists !== undefined || crop.alias === "") {
                // alias is not valid
                crop.hasAliasError = true;
            } else {
                // everything was good:
                crop.hasAliasError = false;
            }
        };

        $scope.confirmChanges = function (crop, event) {
            if (event.keyCode == 13) {
                $scope.setChanges(crop, event);
                event.preventDefault();
            }
        };

        $scope.focusNextField = function (event) {
            if (event.keyCode == 13) {

                var el = event.target;

                var inputs = Array.from(document.querySelectorAll("input:not(disabled)"));
                var inputIndex = inputs.indexOf(el);
                if (inputIndex > -1) {
                    var nextIndex = inputs.indexOf(el) +1;

                    if(inputs.length > nextIndex) {
                        inputs[nextIndex].focus();
                        event.preventDefault();
                    }
                }
            }
        };

        $scope.sortableOptions = {
            axis: 'y',
            containment: 'parent',
            cursor: 'move',
            tolerance: 'pointer'
        };

        $scope.$on("$destroy", function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });

	});
