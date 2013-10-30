function contentTypePicker($scope, entityResource) {

    entityResource.getAll("DocumentType").then(function (data) {
        //convert the ids to strings so the drop downs work properly when comparing
        _.each(data, function(d) {
            d.id = d.id.toString();
        });
        $scope.contentTypes = data;
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
        if ($scope.model.config.multiple) {
            $scope.model.value = $scope.model.value.split(',');
        }
    }
}
angular.module('umbraco').controller("Umbraco.PropertyEditors.ContentTypeController", contentTypePicker);