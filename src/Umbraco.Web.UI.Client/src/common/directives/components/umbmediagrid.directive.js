(function() {
   'use strict';

   function MediaGridDirective($filter, mediaHelper) {

      function link(scope, el, attr, ctrl) {

         var itemDefaultHeight = 200;
         var itemDefaultWidth = 200;
         var itemMaxWidth = 300;
         var itemMaxHeight = 300;

         function activate() {

            for (var i = 0; scope.items.length > i; i++) {
               var item = scope.items[i];
               setItemData(item);
               setOriginalSize(item, itemMaxHeight);
            }

            if(scope.items.length > 0) {
               setFlexValues(scope.items);
            }

         }

         function setItemData(item) {

             item.isFolder = !mediaHelper.hasFilePropertyType(item);
             item.hidden = item.isFolder;

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

         scope.selectItem = function(item, $event, $index) {
            if(scope.onSelect) {
               scope.onSelect(item, $event, $index);
               $event.stopPropagation();
            }
         };

         scope.clickItem = function(item) {
            if(scope.onClick) {
               scope.onClick(item);
            }
         };

         scope.hoverItemDetails = function(item, $event, hover) {
            if(scope.onDetailsHover) {
               scope.onDetailsHover(item, $event, hover);
            }
         };

         var unbindItemsWatcher = scope.$watch('items', function(newValue, oldValue){
            if(angular.isArray(newValue)) {
               activate();
            }
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
            onDetailsHover: "=",
            onSelect: '=',
            onClick: '='
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbMediaGrid', MediaGridDirective);

})();
