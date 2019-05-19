angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.BlockEditor.SettingsController", [
        "$scope",
        "contentTypeResource",
        function ($scope, contentTypeResource) {

            if (!$scope.model.value) {
                $scope.model.value = [];
            }

            $scope.addBlock = function (elementType) {

                var block = {
                    elementType: elementType.udi,
                    settings: null
                };

                $scope.model.value.push(block);
            }

            $scope.elementTypes = [];

            contentTypeResource.getAll()
                .then(function (data) {
                    $scope.elementTypes = _.where(data, { isElement: true });
                });
        }
    ]
);
