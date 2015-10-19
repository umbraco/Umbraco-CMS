(function() {
   'use strict';

   function MediaGridDirective($filter, mediaHelper) {

      function link(scope, el, attr, ctrl) {

         scope.mediaItems = [];

         var itemDefaultHeight = 200;
         var itemDefaultWidth = 200;
         var itemMaxWidth = 300;
         var itemMaxHeight = 300;

         scope.mediaItemsSortingOptions = {
            distance: 10,
            tolerance: "pointer",
            opacity: 0.7,
            scroll: true,
            cursor: "move",
            zIndex: 6000,
            placeholder: "umb-media-grid__placeholder",
            start: function(e, ui) {
              ui.placeholder.height(ui.item.height());
              ui.placeholder.width(ui.item.width());
            }
         };

         function activate() {

            scope.mediaItems = [];

            for (var i = 0; scope.items.length > i; i++) {

               var item = scope.items[i];

               setItemData(item);

               setOriginalSize(item, itemMaxHeight);

               seperateFolderAndMediaItems(item);

            }

            if(scope.mediaItems.length > 0) {
               setFlexValues(scope.mediaItems);
            }

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
             item.width = itemDefaultWidth;
             item.height = itemDefaultHeight;
             item.aspectRatio = 1;

             var widthProp = _.find(item.properties, function(v) { return (v.alias === "umbracoWidth"); });

             if (widthProp && widthProp.value) {
                 item.width = parseInt(widthProp.value, 10);
                 if (isNaN(item.width)) {
                     item.width = itemDefaultWidth;
                 }
             }

             var heightProp = _.find(item.properties, function(v) { return (v.alias === "umbracoHeight"); });

             if (heightProp && heightProp.value) {
                 item.height = parseInt(heightProp.value, 10);
                 if (isNaN(item.height)) {
                     item.height = itemDefaultWidth;
                 }
             }

             item.aspectRatio = item.width / item.height;

             // set max width and height
             if(item.width > itemMaxWidth) {
                item.width = itemMaxWidth;
                item.height = itemMaxWidth / item.aspectRatio;
             }

             if(item.height > itemMaxHeight) {
                item.height = itemMaxHeight;
                item.width = itemMaxHeight * item.aspectRatio;
             }

         }

         function seperateFolderAndMediaItems(item) {

            if(!item.isFolder){
               scope.mediaItems.push(item);
            }

         }

         function setFlexValues(mediaItems) {

            var flexSortArray = mediaItems;
            var smallestImageWidth = null;
            var widestImageAspectRatio = null;

            // sort array after image width with the widest image first
            flexSortArray = $filter('orderBy')(flexSortArray, 'width', true);

            // find widest image aspect ratio
            widestImageAspectRatio = flexSortArray[0].aspectRatio;

            // find smallest image width
            smallestImageWidth = flexSortArray[flexSortArray.length - 1].width;

            for (var i = 0; flexSortArray.length > i; i++) {

               var mediaItem = flexSortArray[i];
               var flex = 1 / (widestImageAspectRatio / mediaItem.aspectRatio);

               if (flex === 0) {
                  flex = 1;
               }

               var imageMinWidth = smallestImageWidth * flex;

               var flexStyle = {
                  "flex": flex + " 1 " + imageMinWidth + "px",
                  "max-width": mediaItem.width + "px"
               };

               mediaItem.flexStyle = flexStyle;

            }

         }

         scope.toggleSelectItem = function(item) {
            item.selected = !item.selected;
         };

         scope.onDetailsOver = function(item, event) {
            if(scope.detailsHover) {
               scope.detailsHover(item, event, true);
            }
         };

         scope.onDetailsOut = function(item, event) {
            if(scope.detailsHover) {
               scope.detailsHover(item, event, false);
            }
         };

         var unbindItemsWatcher = scope.$watch('items', function(newValue, oldValue){
            activate();
         });

         scope.$on('$destroy', function(){
           unbindItemsWatcher();
         });

      }

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/umb-media-grid.html',
         scope: {
            items: '=',
            detailsHover: "="
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbMediaGrid', MediaGridDirective);

})();
