(function() {
   "use strict";

   function EmbedController($scope, $http, $sce, umbRequestHelper, localizationService) {

        var vm = this;
        var origWidth = 500;
        var origHeight = 300;

        vm.loading = false;
        vm.trustedPreview = null;

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

        if ($scope.model.original) {
            angular.extend($scope.model.embed, $scope.model.original);

            showPreview();
        }

        vm.showPreview = showPreview;
        vm.changeSize = changeSize;
        vm.submit = submit;
        vm.close = close;

        function onInit() {
            if(!$scope.model.title) {
                localizationService.localize("general_embed").then(function(value){
                    $scope.model.title = value;
                });
            }
        }

        function showPreview() {

            if ($scope.model.embed.url) {
                $scope.model.embed.show = true;
                $scope.model.embed.info = "";
                $scope.model.embed.success = false;

                vm.loading = true;

                $http({
                    method: 'GET',
                    url: umbRequestHelper.getApiUrl("embedApiBaseUrl", "GetEmbed"),
                    params: {
                        url: $scope.model.embed.url,
                        width: $scope.model.embed.width,
                        height: $scope.model.embed.height
                    }
                }).then(function(response) {

                    $scope.model.embed.preview = "";

                    switch (response.data.OEmbedStatus) {
                        case 0:
                            //not supported
                            $scope.model.embed.info = "Not supported";
                            break;
                        case 1:
                            //error
                            $scope.model.embed.info = "Could not embed media - please ensure the URL is valid";
                            break;
                        case 2:
                            $scope.model.embed.preview = response.data.Markup;
                            vm.trustedPreview = $sce.trustAsHtml(response.data.Markup);
                            $scope.model.embed.supportsDimensions = response.data.SupportsDimensions;
                            $scope.model.embed.success = true;
                            break;
                    }

                    vm.loading = false;

                }, function() {
                    $scope.model.embed.supportsDimensions = false;
                    $scope.model.embed.preview = "";
                    $scope.model.embed.info = "Could not embed media - please ensure the URL is valid";

                    vm.loading = false;
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

       function submit() {
            if($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }

        function close() {
            if($scope.model && $scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();

   }

   angular.module("umbraco").controller("Umbraco.Editors.EmbedController", EmbedController);

})();
