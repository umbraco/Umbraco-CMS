(function () {
    "use strict";

    function InsertFieldController ($scope, $http, contentTypeResource) {
      var vm = this;

      vm.field;
      vm.altField;
      vm.altText;
      vm.insertPreviewTitle = "Modify output";
      vm.insertBefore;
      vm.insertAfter;
      vm.recursive = false;
      vm.properties;
      vm.date = false;
      vm.dateTime = false;
      vm.dateTimeSeparator = "";
      vm.casingUpper = false;
      vm.casingLower = false;
      vm.encodeHtml = false;
      vm.encodeUrl = false;
      vm.convertLinebreaks = false;

      vm.showAltField = false;
      vm.showAltText = false;

      vm.showInsertPreview = showInsertPreview;
      vm.updateInsertPreview = updateInsertPreview;
      vm.hideInsertPreview = hideInsertPreview;
      vm.updateDate = updateDate;
      vm.updateDateTime = updateDateTime;
      vm.updateUpper = updateUpper;
      vm.updateLower = updateLower;
      vm.updateEncodeHtml = updateEncodeHtml;
      vm.updateEncodeUrl = updateEncodeUrl;

      contentTypeResource.getAllPropertyTypeAliases().then(function(array) {
        vm.properties = array;
      });

      //hide preview by default
      function hideInsertPreview () {
        $scope.model.itemDetails = null;
      }

      //Create preview
      function showInsertPreview () {
        var previewDetails = {};

        previewDetails.title = vm.insertPreviewTitle;

        if (!vm.insertBefore && !vm.insertAfter) {
            previewDetails.description = $scope.model.umbracoField
        } else if (!vm.insertAfter) {
            previewDetails.description = vm.insertBefore + ' ' + $scope.model.umbracoField;
        } else {
          previewDetails.description = vm.insertBefore + ' ' + $scope.model.umbracoField + ' ' + vm.insertAfter;
        }
        $scope.model.itemDetails = previewDetails;
      }

      //Update preview
      function updateInsertPreview () {
        var previewDetails = $scope.model.itemDetails;

        previewDetails.title = vm.insertPreviewTitle;

        if (!vm.insertBefore && !vm.insertAfter) {
            previewDetails.description = ' ' + $scope.model.umbracoField;
        } else if (!vm.insertAfter) {
            previewDetails.description = vm.insertBefore + ' ' + $scope.model.umbracoField;
        } else {
          previewDetails.description = vm.insertBefore + ' ' + $scope.model.umbracoField + ' ' + vm.insertAfter;
        }

        $scope.model.itemDetails = previewDetails;
      }

      // date formatting
      function updateDate () {
        if (vm.date) {
          vm.date = false;
          return;
        }else {
          vm.date = true;
          if (vm.dateTime) {
            vm.dateTime = false;
          }
        }
      }

      function updateDateTime () {
        if (vm.dateTime) {
          vm.dateTime = false
        } else {
          vm.dateTime = true;
          if (vm.date) {
            vm.date = false;
          }
        }
      }

      // casing
      function updateUpper() {
        if (vm.casingUpper) {
          vm.casingUpper = false;
          return;
        }else {
          vm.casingUpper = true;
          if (vm.casingLower) {
            vm.casingLower = false;
          }
        }
      }

      function updateLower() {
        if (vm.casingLower) {
          vm.casingLower = false;
          return;
        }else {
          vm.casingLower = true;
          if (vm.casingUpper) {
            vm.casingUpper = false;
          }
        }
      }

      // encoding
      function updateEncodeHtml() {
        if (vm.encodeHtml) {
          vm.encodeHtml = false;
          return;
        }else {
          vm.encodeHtml = true;
          if (vm.encodeUrl) {
            vm.encodeUrl = false;
          }
        }
      }

      function updateEncodeUrl() {
        if (vm.encodeUrl) {
          vm.encodeUrl = false;
          return;
        } else {
          vm.encodeUrl = true;
          if (vm.encodeHtml) {
            vm.encodeHtml = false;
          }
        }
      }

      $scope.updatePageField = function() {
        var pageField =     (vm.field !== undefined ? '@Umbraco.Field("' + vm.field + '"' : "")
                          + (vm.altField !== undefined ? ', altFieldAlias:"' + vm.altField + '"': "")
                          + (vm.altText !== undefined ? ', altText:"' + vm.altText + '"' : "")
                          + (vm.insertBefore !== undefined ? ', insertBefore:"' + vm.insertBefore + '"' : "")
                          + (vm.insertAfter !== undefined ? ', insertAfter:"' + vm.insertAfter + '"' : "")
                          + (vm.recursive !== false ? ', recursive: ' + vm.recursive : "")
                          + (vm.date !== false ? ', formatAsDate: ' + vm.date : "")
                          + (vm.dateTime !== false ? ', formatAsDateWithTimeSeparator:"' + vm.dateTimeSeparator + '"' : "")
                          + (vm.casingUpper !== false ? ', casing: ' + "RenderFieldCaseType.Upper" : "")
                          + (vm.casingLower !== false ? ', casing: ' + "RenderFieldCaseType.Lower" : "")
                          + (vm.encodeHtml !== false ? ', encoding: ' + "RenderFieldEncodingType.Html" : "")
                          + (vm.encodeUrl !== false ? ', encoding: ' + "RenderFieldEncodingType.Url" : "")
                          + (vm.convertLinebreaks !== false ? ', convertLineBreaks: ' + "true" : "")
                          + (vm.field ? ')' : "");
        $scope.model.umbracoField = pageField;
        return pageField;
      };


    }

    angular.module("umbraco").controller("Umbraco.Overlays.InsertFieldController", InsertFieldController);
})();
