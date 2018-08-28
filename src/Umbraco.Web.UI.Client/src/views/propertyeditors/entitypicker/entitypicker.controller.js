/** A drop down list or multi value select list based on an entity type, this can be re-used for any entity types */
function entityPicker($scope, entityResource) {

    //set the default to DocumentType
    if (!$scope.model.config.entityType) {
        $scope.model.config.entityType = "DocumentType";
    }

    //Determine the select list options and which value to publish
    if (!$scope.model.config.publishBy) {
        $scope.selectOptions = "entity.id as entity.name for entity in entities";
    }
    else {
        $scope.selectOptions = "entity." + $scope.model.config.publishBy + " as entity.name for entity in entities";
    }

    entityResource.getAll($scope.model.config.entityType).then(function (data) {
        //convert the ids to strings so the drop downs work properly when comparing
        _.each(data, function(d) {
            d.id = d.id.toString();
        });
        $scope.entities = data;
    });

    if ($scope.model.value === null || $scope.model.value === undefined) {
        if ($scope.model.config.multiple) {
            $scope.model.value = [];
        }
        else {
            $scope.model.value = "";
        }
    }
    else {
        //if it's multiple, change the value to an array
        if (Object.toBoolean($scope.model.config.multiple)) {
            if (_.isString($scope.model.value)) {
                $scope.model.value = $scope.model.value.split(',');
            }
        }
    }
}
angular.module('umbraco').controller("Umbraco.PropertyEditors.EntityPickerController", entityPicker);
