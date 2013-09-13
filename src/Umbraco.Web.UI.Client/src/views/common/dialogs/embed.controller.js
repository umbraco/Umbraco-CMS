angular.module("umbraco").controller("Umbraco.Dialogs.EmbedController", function ($scope, $http) {
    $scope.url = "";
    $scope.width = 500;
    $scope.height = 300;
    $scope.constrain = true;
    $scope.preview = "";

    $scope.preview = function () {

        $http({ method: 'POST', url: '/umbraco/UmbracoApi/Embed/Embed', params: { url: $scope.url, width: $scope.width, height: $scope.height } })
            .success(function (data) {
                $scope.preview = data.Markup;
            })
            .error(function () {

            });

    };

    $scope.insert = function () {
        $scope.submit($scope.preview);
    };
});