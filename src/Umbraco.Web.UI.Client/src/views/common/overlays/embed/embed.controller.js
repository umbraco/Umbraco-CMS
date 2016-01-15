(function() {
   "use strict";

   function EmbedOverlay($scope, $http, umbRequestHelper, localizationService) {

      var vm = this;
      var origWidth = 500;
      var origHeight = 300;

      if(!$scope.model.title) {
          $scope.model.title = localizationService.localize("general_embed");
      }

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

      vm.showPreview = showPreview;
      vm.changeSize = changeSize;

      function showPreview() {

         if ($scope.model.embed.url) {
            $scope.model.embed.show = true;
            $scope.model.embed.preview = "<div class=\"umb-loader\" style=\"height: 10px; margin: 10px 0px;\"></div>";
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
               })
               .success(function(data) {

                  $scope.model.embed.preview = "";

                  switch (data.Status) {
                     case 0:
                        //not supported
                        $scope.model.embed.info = "Not supported";
                        break;
                     case 1:
                        //error
                        $scope.model.embed.info = "Computer says no";
                        break;
                     case 2:
                        $scope.model.embed.preview = data.Markup;
                        $scope.model.embed.supportsDimensions = data.SupportsDimensions;
                        $scope.model.embed.success = true;
                        break;
                  }
               })
               .error(function() {
                  $scope.model.embed.supportsDimensions = false;
                  $scope.model.embed.preview = "";
                  $scope.model.embed.info = "Computer says no";
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

   }

   angular.module("umbraco").controller("Umbraco.Overlays.EmbedOverlay", EmbedOverlay);

})();
