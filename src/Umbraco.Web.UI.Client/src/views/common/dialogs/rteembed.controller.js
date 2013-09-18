angular.module("umbraco").controller("Umbraco.Dialogs.RteEmbedController", function ($scope, $http) {
    $scope.url = "";
    $scope.width = 500;
    $scope.height = 300;
    $scope.constrain = true;
    $scope.preview = "";
    $scope.success = false;
    $scope.info = "";
    $scope.supportsDimensions = false;
    
    var origWidth = 500;
    var origHeight = 300;
    
    $scope.showPreview = function(){

        if ($scope.url != "") {
            
            $scope.preview = "<div class=\"umb-loader\">";
            $scope.info = "";
            $scope.success = false;

            $http({ method: 'GET', url: '/umbraco/UmbracoApi/RteEmbed/GetEmbed', params: { url: $scope.url, width: $scope.width, height: $scope.height } })
                .success(function (data) {
                    
                    $scope.preview = "";
                    
                    switch (data.Status) {
                        case 0:
                            //not supported
                            $scope.info = "Not Supported";
                            break;
                        case 1:
                            //error
                            $scope.info = "Computer says no";
                            break;
                        case 2:
                            $scope.preview = data.Markup;
                            $scope.supportsDimensions = data.SupportsDimensions;
                            $scope.success = true;
                            break;
                    }
                })
                .error(function() {
                    $scope.preview = "";
                    $scope.info = "Computer says no";
                });

        }

    };

    $scope.changeSize = function (type) {
        var width, height;
        
        if ($scope.constrain) {
            width = parseInt($scope.width, 10);
            height = parseInt($scope.height, 10);
            if (type == 'width') {
                origHeight = Math.round((width / origWidth) * height);
                $scope.height = origHeight;
            } else {
                origWidth = Math.round((height / origHeight) * width);
                $scope.width = origWidth;
            }
        }
        if ($scope.url != "") {
            $scope.showPreview();
        }

    };
    
    $scope.insert = function(){
        $scope.submit($scope.preview);
    };
});