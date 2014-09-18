angular.module("umbraco").controller("Umbraco.PrevalueEditors.NestedPropertyController",
    function ($scope, entityResource) {

        if (!$scope.model.value) {
            $scope.model.value = [];
        }

        $scope.add = function () {
            $scope.model.value.push({ alias: '', title: '', descriptoin: '', datatype: 'textstring', required: false });
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

        var setAliasDebounce = _.debounce(function (index) {
            $scope.model.value[index].alias = $scope.model.value[index].label.substring(0, 1).toLowerCase() + $scope.model.value[index].label.replace(/\s+/, '').slice(1);
        }, 500);

        $scope.setAlias = function (index) {
            setAliasDebounce(index);
        }

        entityResource.getAll("Datatype").then(function (data) {
            $scope.datatypes = data;
        });

    });