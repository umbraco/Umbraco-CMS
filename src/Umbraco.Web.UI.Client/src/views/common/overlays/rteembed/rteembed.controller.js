(function() {
   "use strict";

   function RteEmbedOverlay($scope, $http, umbRequestHelper) {

      var vm = this;
      var origWidth = 500;
      var origHeight = 300;

      $scope.model.title = "Embed";
      $scope.model.form = {
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

         if ($scope.model.form.url) {
            $scope.model.form.show = true;
            $scope.model.form.preview = "<div class=\"umb-loader\" style=\"height: 10px; margin: 10px 0px;\"></div>";
            $scope.model.form.info = "";
            $scope.model.form.success = false;

            $http({
                  method: 'GET',
                  url: umbRequestHelper.getApiUrl("embedApiBaseUrl", "GetEmbed"),
                  params: {
                     url: $scope.model.form.url,
                     width: $scope.model.form.width,
                     height: $scope.model.form.height
                  }
               })
               .success(function(data) {

                  $scope.model.form.preview = "";

                  switch (data.Status) {
                     case 0:
                        //not supported
                        $scope.model.form.info = "Not supported";
                        break;
                     case 1:
                        //error
                        $scope.model.form.info = "Computer says no";
                        break;
                     case 2:
                        $scope.model.form.preview = data.Markup;
                        $scope.model.form.supportsDimensions = data.SupportsDimensions;
                        $scope.model.form.success = true;
                        break;
                  }
               })
               .error(function() {
                  $scope.model.form.supportsDimensions = false;
                  $scope.model.form.preview = "";
                  $scope.model.form.info = "Computer says no";
               });
         } else {
            $scope.model.form.supportsDimensions = false;
            $scope.model.form.preview = "";
            $scope.model.form.info = "Please enter a URL";
         }
      }

      function changeSize(type) {

         var width, height;

         if ($scope.model.form.constrain) {
            width = parseInt($scope.model.form.width, 10);
            height = parseInt($scope.model.form.height, 10);
            if (type == 'width') {
               origHeight = Math.round((width / origWidth) * height);
               $scope.model.form.height = origHeight;
            } else {
               origWidth = Math.round((height / origHeight) * width);
               $scope.model.form.width = origWidth;
            }
         }
         if ($scope.model.form.url !== "") {
            showPreview();
         }

      }

   }

   angular.module("umbraco").controller("Umbraco.Overlays.RteEmbedOverlay", RteEmbedOverlay);

})();
