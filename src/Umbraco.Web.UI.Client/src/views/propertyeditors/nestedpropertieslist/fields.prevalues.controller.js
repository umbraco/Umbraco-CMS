angular.module("umbraco").controller("Umbraco.PrevalueEditors.NestedPropertyController",
function ($scope, entityResource, angularHelper) {

    if (!$scope.model.value) {
        $scope.model.value = [];
    }

    $scope.add = function () {
        $scope.model.value.push({ alias: '', title: '', descriptoin: '', datatype: 'textstring', required: false, showsummary: false });
        var currForm = angularHelper.getCurrentForm($scope);
        currForm.$setDirty();
    };

    $scope.remove = function (index) {
        var remainder = [];
        for (var x = 0; x < $scope.model.value.length; x++) {
            if (x !== index) {
                remainder.push($scope.model.value[x]);
            }
        }
        $scope.model.value = remainder;
        var currForm = angularHelper.getCurrentForm($scope);
        currForm.$setDirty();
    };

    var setAliasDebounce = _.debounce(function (index) {
        if ($scope.model.value[index].label)
            $scope.model.value[index].alias = $scope.model.value[index].label.substring(0, 1).toLowerCase() + $scope.model.value[index].label.replace(/\s+/, '').slice(1);
    }, 50);

    $scope.setAlias = function (index) {
        setAliasDebounce(index);
    }

    entityResource.getAll("Datatype").then(function (data) {
        $scope.datatypes = data;
    });

    $scope.sortableOptions = {
        axis: 'y',
        containment: 'parent',
        cursor: 'move',
        items: '> div.control-group',
        tolerance: 'pointer',
        update: function (e, ui) {
            // Get the new and old index for the moved element (using the text as the identifier, so 
            // we'd have a problem if two prevalues were the same, but that would be unlikely)
            var newIndex = ui.item.index();
            var movedPrevalueText = $('input[type="text"][ng-model="item.alias"]', ui.item).val();
            var originalIndex = getElementIndexByPrevalueText(movedPrevalueText);

            // Move the element in the model
            if (originalIndex > -1) {
                var movedElement = $scope.model.value[originalIndex];
                $scope.model.value.splice(originalIndex, 1);
                $scope.model.value.splice(newIndex, 0, movedElement);
            }

            var currForm = angularHelper.getCurrentForm($scope);
            currForm.$setDirty();
        }
    };


    function getElementIndexByPrevalueText(value) {
        for (var i = 0; i < $scope.model.value.length; i++) {
            if ($scope.model.value[i].alias === value) {
                return i;
            }
        }

        return -1;
    }
});