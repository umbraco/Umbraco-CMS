(function() {
   'use strict';

   function MediaGridDirective($filter, mediaHelper) {

      function link(scope, el, attr, ctrl) {

         scope.folders = [];
         scope.mediaItems = [];
         var itemMaxHeight = 200;

         function activate() {

            for (var i = 0; scope.items.length > i; i++) {

               var item = scope.items[i];

               setItemData(item);

               setOriginalSize(item, itemMaxHeight);

               seperateFolderAndMediaItems(item);

            }

            setFlexValues(scope.items);

         }

         function setItemData(item) {

             item.isFolder = !mediaHelper.hasFilePropertyType(item);

             if(!item.isFolder){
                 item.thumbnail = mediaHelper.resolveFile(item, true);
                 item.image = mediaHelper.resolveFile(item, false);
             }
         }

         function setOriginalSize(item, maxHeight) {

             //set to a square by default
             item.originalWidth = maxHeight;
             item.originalHeight = maxHeight;
             item.aspectRatio = 1;

             var widthProp = _.find(item.properties, function(v) { return (v.alias === "umbracoWidth"); });

             if (widthProp && widthProp.value) {
                 item.originalWidth = parseInt(widthProp.value, 10);
                 if (isNaN(item.originalWidth)) {
                     item.originalWidth = maxHeight;
                 }
             }

             var heightProp = _.find(item.properties, function(v) { return (v.alias === "umbracoHeight"); });

             if (heightProp && heightProp.value) {
                 item.originalHeight = parseInt(heightProp.value, 10);
                 if (isNaN(item.originalHeight)) {
                     item.originalHeight = maxHeight;
                 }
             }

             item.aspectRatio = item.originalWidth / item.originalHeight;

         }

         function seperateFolderAndMediaItems(item) {

            if(item.isFolder){
               scope.folders.push(item);
            } else {
               scope.mediaItems.push(item);
            }

         }

         function setFlexValues(items) {

            var flexSortArray = scope.mediaItems;
            var smallestImageWidth = null;
            var widestImageAspectRatio = null;

            // sort array after image width with the widest image first
            flexSortArray = $filter('orderBy')(flexSortArray, 'originalWidth', true);

            // find widest image aspect ratio
            widestImageAspectRatio = flexSortArray[0].aspectRatio;

            // find smallest image width
            smallestImageWidth = flexSortArray[flexSortArray.length - 1].originalWidth;

            for (var i = 0; flexSortArray.length > i; i++) {

               var mediaItem = flexSortArray[i];
               var flex = 1 / (widestImageAspectRatio / mediaItem.aspectRatio);

               if (flex === 0) {
                  flex = 1;
               }

               var imageMinWidth = smallestImageWidth * flex;

               var flexStyle = {
                  "flex": flex + " 1 " + imageMinWidth + "px",
                  "max-width": mediaItem.originalWidth + "px"
               };

               mediaItem.flexStyle = flexStyle;

            }

         }

         scope.toggleSelectItem = function(item) {
            item.selected = !item.selected;
         };

         activate();

      }

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/umb-media-grid.html',
         scope: {
            items: '='
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbMediaGrid', MediaGridDirective);

})();
