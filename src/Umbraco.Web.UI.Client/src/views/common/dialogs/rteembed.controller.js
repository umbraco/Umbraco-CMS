angular.module("umbraco").controller("Umbraco.Dialogs.RteEmbedController", function ($scope, $http, umbRequestHelper) {
    $scope.form = {};
    $scope.form.url = "";
    $scope.form.width = 360;
    $scope.form.height = 240;
    $scope.form.constrain = true;
    $scope.form.preview = "";
    $scope.form.success = false;
    $scope.form.info = "";
    $scope.form.supportsDimensions = false;
    
    var origWidth = 500;
    var origHeight = 300;
    
    $scope.showPreview = function() {

        if ($scope.form.url) {
            $scope.form.show = true;
            $scope.form.preview = "<div class=\"umb-loader\" style=\"height: 10px; margin: 10px 0px;\"></div>";
            $scope.form.info = "";
            $scope.form.success = false;

            $http({ method: 'GET', url: umbRequestHelper.getApiUrl("embedApiBaseUrl", "GetEmbed"), params: { url: $scope.form.url, width: $scope.form.width, height: $scope.form.height } })
                .success(function (data) {
                    
                    $scope.form.preview = "";
                    
                    switch (data.Status) {
                        case 0:
                            //not supported
                            $scope.form.info = "Not supported";
                            break;
                        case 1:
                            //error
                            $scope.form.info = "Computer says no";
                            break;
                        case 2:
                            $scope.form.preview = data.Markup;
                            $scope.form.supportsDimensions = data.SupportsDimensions;
                            $scope.form.success = true;
                            break;
                    }
                })
                .error(function () {
                    $scope.form.supportsDimensions = false;
                    $scope.form.preview = "";
                    $scope.form.info = "Computer says no";
                });
        } else {
            $scope.form.supportsDimensions = false;
            $scope.form.preview = "";
            $scope.form.info = "Please enter a URL";
        }
    };

    $scope.changeSize = function (type) {
        var width, height;
        
        if ($scope.form.constrain) {
            width = parseInt($scope.form.width, 10);
            height = parseInt($scope.form.height, 10);
            if (type == 'width') {
                origHeight = Math.round((width / origWidth) * height);
                $scope.form.height = origHeight;
            } else {
                origWidth = Math.round((height / origHeight) * width);
                $scope.form.width = origWidth;
            }
        }
        if ($scope.form.url != "") {
            $scope.showPreview();
        }

    };
    
    $scope.insert = function(){
        $scope.submit($scope.form.preview);
    };
});