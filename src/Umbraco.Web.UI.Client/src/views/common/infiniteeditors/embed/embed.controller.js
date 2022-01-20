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
            a11yInfo: "",
            supportsDimensions: false,
            originalWidth: 360,
            originalHeight: 240
   };

        if ($scope.model.modify) {
            Utilities.extend($scope.model.embed, $scope.model.modify);

            showPreview();
        }

        vm.toggleConstrain = toggleConstrain;
        vm.showPreview = showPreview;
        vm.changeSize = changeSize;
        vm.submit = submit;
        vm.close = close;
        
       function onInit() {
            if (!$scope.model.title) {
                localizationService.localize("general_embed").then(function(value){
                    $scope.model.title = value;
                });
            }
        }

        function showPreview() {

            if ($scope.model.embed.url) {
                $scope.model.embed.show = true;
                $scope.model.embed.info = "";
                $scope.model.embed.a11yInfo = "";
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
                        case 'NotSupported':
                            //not supported
                            $scope.model.embed.preview = "";
                            $scope.model.embed.info = "Not supported";
                            $scope.model.embed.a11yInfo = $scope.model.embed.info;
                            $scope.model.embed.success = false;
                            $scope.model.embed.supportsDimensions = false;
                            vm.trustedPreview = null;
                            break;
                        case 1:
                        case 'Error':
                            //error
                            $scope.model.embed.preview = "";
                            $scope.model.embed.info = "Could not embed media - please ensure the URL is valid";
                            $scope.model.embed.a11yInfo = $scope.model.embed.info;
                            $scope.model.embed.success = false;
                            $scope.model.embed.supportsDimensions = false;
                            vm.trustedPreview = null;
                            break;
                        case 2:
                        case 'Success':
                            $scope.model.embed.success = true;
                            $scope.model.embed.supportsDimensions = response.data.SupportsDimensions;
                            $scope.model.embed.preview = response.data.Markup;
                            $scope.model.embed.info = "";
                            $scope.model.embed.a11yInfo = "Retrieved URL";
                            vm.trustedPreview = $sce.trustAsHtml(response.data.Markup);
                            break;
                    }

                    vm.loading = false;

                }, function() {
                    $scope.model.embed.success = false;
                    $scope.model.embed.supportsDimensions = false;
                    $scope.model.embed.preview = "";
                    $scope.model.embed.info = "Could not embed media - please ensure the URL is valid";
                    $scope.model.embed.a11yInfo = $scope.model.embed.info;
                    vm.loading = false;
                });
            } else {
                $scope.model.embed.supportsDimensions = false;
                $scope.model.embed.preview = "";
                $scope.model.embed.info = "Please enter a URL";
                $scope.model.embed.a11yInfo = $scope.model.embed.info;
            }
        }

       function changeSize(type) {

           var width = parseInt($scope.model.embed.width, 10);
           var height = parseInt($scope.model.embed.height, 10);
           var originalWidth = parseInt($scope.model.embed.originalWidth, 10);
           var originalHeight = parseInt($scope.model.embed.originalHeight, 10);
           var resize = originalWidth !== width || originalHeight !== height;

           if ($scope.model.embed.constrain) {

               if (type === 'width') {
                   origHeight = Math.round((width / origWidth) * height);
                   $scope.model.embed.height = origHeight;
               } else {
                   origWidth = Math.round((height / origHeight) * width);
                   $scope.model.embed.width = origWidth;
               }
           }
           $scope.model.embed.originalWidth = $scope.model.embed.width;
           $scope.model.embed.originalHeight = $scope.model.embed.height;
           if ($scope.model.embed.url !== "" && resize) {
               showPreview();
           }

       }

       function toggleConstrain() {
           $scope.model.embed.constrain = !$scope.model.embed.constrain;
       }

       function submit() {
            if ($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }

        function close() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close();
            }
       }

        onInit();
   }

   angular.module("umbraco").controller("Umbraco.Editors.EmbedController", EmbedController);

})();
