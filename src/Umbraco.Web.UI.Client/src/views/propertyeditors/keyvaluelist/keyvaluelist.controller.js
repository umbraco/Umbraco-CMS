function KeyValueListController($scope) {

    if (!$scope.model.value) {
        $scope.model.value = {  };
    }
    $scope.value = _.map($scope.model.value, function (v, k) {
        return { key: k, value: v };
    });

    //add any fields that there isn't values for
    if ($scope.model.config.min > 0) {
        for (var i = 0; i < $scope.model.config.min; i++) {
            if ((i + 1) > $scope.value.length) {
                $scope.value.push({ key: '', value: '' });
            }
        }
    }

    $scope.add = function () {
        if ($scope.model.config.max <= 0 || $scope.value.length < $scope.model.config.max) {
            $scope.value.push({ key: '', value: '' });
        }
    };

    $scope.remove = function (index) {
        var remainder = [];
        for (var x = 0; x < $scope.value.length; x++) {
            if (x !== index) {
                remainder.push($scope.value[x]);
            }
        }
        $scope.value = remainder;
    };

    $scope.$on("formSubmitting", function (ev, args) {
        $scope.model.value = _.reduce($scope.value, function (o, v, i) {
            o[v.key] = v.value;
            return o;
        }, {});
    });
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.KeyValueListController", KeyValueListController);