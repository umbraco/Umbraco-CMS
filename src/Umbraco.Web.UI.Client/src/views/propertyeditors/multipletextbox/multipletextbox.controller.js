function MultipleTextBoxController($scope) {

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

        var e = angular.element(elem[0].querySelector('[name="item_0"]'));

        console.log(e);

        switch (event.keyCode) {
            case 13:
                if ($scope.model.config.max <= 0 || $scope.model.value.length < $scope.model.config.max) {
                    $scope.model.value.push({ value: "" });

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
                    }
                }
                break;
            default:
        }
    }



    $scope.add = function () {
        if ($scope.model.config.max <= 0 || $scope.model.value.length < $scope.model.config.max) {
            $scope.model.value.push({ value: "" });
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
    };

}

angular.module("umbraco").controller("Umbraco.PropertyEditors.MultipleTextBoxController", MultipleTextBoxController);
