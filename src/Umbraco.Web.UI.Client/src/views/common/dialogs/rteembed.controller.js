angular.module("umbraco").controller("Umbraco.Dialogs.RteEmbedController", function ($scope, $http) {
    $scope.url = "";
    $scope.width = 500;
    $scope.height = 300;
    $scope.constrain = true;
    $scope.preview = "";
    $scope.success = false;
    
    $scope.preview = function(){

        if ($scope.url != "") {
            
            $scope.preview = "<div class=\"umb-loader\">";
            
            $scope.success = false;

            $http({ method: 'GET', url: '/umbraco/UmbracoApi/RteEmbed/GetEmbed', params: { url: $scope.url, width: $scope.width, height: $scope.height } })
                .success(function(data) {
                    $scope.preview = data.Markup;
                    $scope.success = true;
                })
                .error(function() {
                    $scope.preview = "";
                });

        }

    };

    $scope.insert = function(){
        $scope.submit($scope.preview);
    };
});