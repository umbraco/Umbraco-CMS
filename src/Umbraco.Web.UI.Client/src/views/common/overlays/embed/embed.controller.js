(function () {
    "use strict";

    function EmbedOverlay($scope, $http, $sce, umbRequestHelper, localizationService) {

        var origWidth = 500;
        var origHeight = 300;

        $scope.model.embed = {
            url: "",
            width: 360,
            height: 240,
            constrain: true,
            preview: "",
            success: false,
            info: "",
            supportsDimensions: ""
        };

        $scope.showPreview = showPreview;
        $scope.changeSize = changeSize;

        function onInit() {
            if (!$scope.model.title) {
                localizationService.localize("general_embed").then(function (value) {
                    $scope.model.title = value;
                });
            }
        }

        function showPreview() {

            if ($scope.model.embed.url) {
                $scope.model.embed.show = true;
                $scope.model.embed.preview = "<div class=\"umb-loader\" style=\"height: 10px; margin: 10px 0px;\"></div>";
                $scope.model.embed.trustedPreview = $scope.model.embed.preview;
                $scope.model.embed.info = "";
                $scope.model.embed.success = false;

                $http({
                    method: 'GET',
                    url: umbRequestHelper.getApiUrl("embedApiBaseUrl", "GetEmbed"),
                    params: {
                        url: $scope.model.embed.url,
                        width: $scope.model.embed.width,
                        height: $scope.model.embed.height
                    }
                }).then(function (data) {
                    $scope.model.embed.preview = "";

                    switch (data.data.Status) {
                        case 0:
                            //not supported
                            $scope.model.embed.info = "Not supported";
                            break;
                        case 1:
                            //error
                            $scope.model.embed.info = "Could not embed media - please ensure the URL is valid";
                            break;
                        case 2:
                            $scope.model.embed.preview = data.data.Markup
                            $scope.model.embed.trustedPreview = $sce.trustAsHtml($scope.model.embed.preview);
                            $scope.model.embed.supportsDimensions = data.data.SupportsDimensions;
                            $scope.model.embed.success = true;
                            break;
                    }
                },
                    function () {
                        $scope.model.embed.supportsDimensions = false;
                        $scope.model.embed.preview = "";
                        $scope.model.embed.info = "Could not embed media - please ensure the URL is valid";
                    });
            } else {
                $scope.model.embed.supportsDimensions = false;
                $scope.model.embed.preview = "";
                $scope.model.embed.info = "Please enter a URL";
            }
        }

        function changeSize(type) {

            var width, height;

            if ($scope.model.embed.constrain) {
                width = parseInt($scope.model.embed.width, 10);
                height = parseInt($scope.model.embed.height, 10);
                if (type == 'width') {
                    origHeight = Math.round((width / origWidth) * height);
                    $scope.model.embed.height = origHeight;
                } else {
                    origWidth = Math.round((height / origHeight) * width);
                    $scope.model.embed.width = origWidth;
                }
            }
            if ($scope.model.embed.url !== "") {
                showPreview();
            }

        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.EmbedOverlay", EmbedOverlay);

})();
