angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.BlockEditor.SettingsController", [
        "$scope",
        "contentTypeResource",
        function ($scope, contentTypeResource) {
            $scope.model.value = [];

            $scope.elementTypes = [];

            contentTypeResource.getAll()
                .then(function (data) {
                    $scope.elementTypes = _.where(data, { isElement: true });
                });
        }
    ]
);
