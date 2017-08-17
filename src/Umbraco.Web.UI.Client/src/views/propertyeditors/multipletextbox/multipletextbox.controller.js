function MultipleTextBoxController($scope, $timeout) {

    $scope.sortableOptions = {
        axis: 'y',
        containment: 'parent',
        cursor: 'move',
        items: '> div.control-group',
        tolerance: 'pointer'
    };

    if (!$scope.model.value) {
        $scope.model.value = [];
    }

    //add any fields that there isn't values for
    if ($scope.model.config.min > 0) {
        for (var i = 0; i < $scope.model.config.min; i++) {
            if ((i + 1) > $scope.model.value.length) {
                $scope.model.value.push({ value: "" });
            }
        }
    }

    //TODO add focus to newly created text box or the first in line after deletion
    $scope.addRemoveOnKeyDown = function (event, index) {

        var txtBoxValue = $scope.model.value[index];

        var newItemIndex;

        event.preventDefault();

        switch (event.keyCode) {
            case 13:
                if ($scope.model.config.max <= 0 || $scope.model.value.length < $scope.model.config.max) {
                    $scope.model.value.push({ value: "" });

                    newItemIndex = $scope.model.value.length - 1;

                    //Focus on the newly added value
                    $scope.model.value[newItemIndex].hasFocus = true;
                }
                break;
            case 8:

                var remainder = [];
                if ($scope.model.value.length > 1) {
                    if (txtBoxValue.value === "") {
                        for (var x = 0; x < $scope.model.value.length; x++) {
                            if (x !== index) {
                                remainder.push($scope.model.value[x]);
                            }
                        }

                        $scope.model.value = remainder;

                        newItemIndex = $scope.model.value.length - 1;

                        //Set focus back on false as the directive only watches for true
                        $scope.model.value[newItemIndex].hasFocus = false;

                        $timeout(function () {
                            //Focus on the previous value
                            $scope.model.value[newItemIndex].hasFocus = true;
                        });
                    }
                }
                break;
            default:
        }
    }



    $scope.add = function () {
        if ($scope.model.config.max <= 0 || $scope.model.value.length < $scope.model.config.max) {
            $scope.model.value.push({ value: "" });
            $scope.focusMe = false;
        }
    };

    $scope.remove = function (index) {
        var remainder = [];
        for (var x = 0; x < $scope.model.value.length; x++) {
            if (x !== index) {
                remainder.push($scope.model.value[x]);
            }
        }
        $scope.model.value = remainder;
        $scope.focusMe = true;
    };

}

angular.module("umbraco").controller("Umbraco.PropertyEditors.MultipleTextBoxController", MultipleTextBoxController);