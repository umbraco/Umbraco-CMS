/**
* @ngdoc directive
* @name umbraco.directives.directive:umbPhotoFolder
* @restrict E
**/
angular.module("umbraco.directives.html")
.directive('umbPhotoFolder', function ($compile, $log, $timeout, $filter, imageHelper, umbRequestHelper) {

    /** sets the image's url - will check if it is a folder or a real image */
    function setImageUrl(img) {
        //get the image property (if one exists)
        var imageProp = imageHelper.getImagePropertyValue({ imageModel: img });
        if (!imageProp) {
            img.thumbnail = "none";
        }
        else {

            //get the proxy url for big thumbnails (this ensures one is always generated)
            var thumbnailUrl = umbRequestHelper.getApiUrl(
                "imagesApiBaseUrl",
                "GetBigThumbnail",
                [{ mediaId: img.id }]);
            img.thumbnail = thumbnailUrl;
        }
    }
    
    /** sets the images original size properties - will check if it is a folder and if so will just make it square */
    function setOriginalSize(img, maxHeight) {
        //set to a square by default
        img.originalWidth = maxHeight;
        img.originalHeight = maxHeight;

        var widthProp = _.find(img.properties, function (v) { return (v.alias === "umbracoWidth"); });
        if (widthProp && widthProp.value) {
            img.originalWidth = parseInt(widthProp.value, 10);
            if (isNaN(img.originalWidth)) {
                img.originalWidth = maxHeight;
            }
        }
        var heightProp = _.find(img.properties, function (v) { return (v.alias === "umbracoHeight"); });
        if (heightProp && heightProp.value) {
            img.originalHeight = parseInt(heightProp.value, 10);
            if (isNaN(img.originalHeight)) {
                img.originalHeight = maxHeight;
            }
        }
    }

    /** sets the image style which get's used in the angular markup */
    function setImageStyle(img, width, height, rightMargin, bottomMargin) {
        img.style = { width: width, height: height, "margin-right": rightMargin + "px", "margin-bottom": bottomMargin + "px" };
        img.thumbStyle = {
            "background-image": "url('" + img.thumbnail + "')",
            "background-repeat": "no-repeat",
            "background-position": "center",
            "background-size": Math.min(width, img.originalWidth) + "px " + Math.min(height, img.originalHeight) + "px"
        };
    }

    /** gets the image's scaled wdith based on the max row height and width */
    function getScaledWidth(img, maxHeight, maxRowWidth) {
        var scaled = img.originalWidth * maxHeight / img.originalHeight;
        //round down, we don't want it too big even by half a pixel otherwise it'll drop to the next row
        var rounded = Math.floor(scaled);
        //if the max row width is smaller than the scaled width of the image then we better make
        // the scaled width the max row width so an image can actually fit on the row
        return Math.min(rounded, maxRowWidth);
    }
    
    /** 
        This will determine the row/image height for the next collection of images which takes into account the 
        ideal image count per row. It will check if a row can be filled with this ideal count and if not - if there
        are additional images available to fill the row it will keep calculating until they fit.
    */
    function getRowHeight(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin) {

        var currRowWidth = 0;
        var idealImages = imgs.slice(0, idealImgPerRow);
        //take into account the margin, we will have 1 less margin item than we have total images
        // easiest to just reduce the maxRowWidth by that number
        var targetRowWidth = maxRowWidth - ((idealImages.length -1) * margin);
        var maxScaleableHeight = getMaxScaleableHeight(idealImages, maxRowHeight);
        var targetHeight = Math.max(maxScaleableHeight, minDisplayHeight);

        for (var i = 0; i < idealImages.length; i++) {            
            currRowWidth += getScaledWidth(idealImages[i], targetHeight, targetRowWidth);
        }
        
        if (currRowWidth > targetRowWidth) {
            //get the ratio
            var r = targetRowWidth / currRowWidth;
            var newHeight = targetHeight * r;
            //if this becomes smaller than the min display then we need to use the min display,
            if (newHeight < minDisplayHeight) {
                newHeight = minDisplayHeight;
            }
            //always take the rounded down width so we know it will fit
            return Math.floor(newHeight);
        }
        else {
            
            //we know the width will fit in a row, but we now need to figure out if we can fill 
            // the entire row in the case that we have more images remaining than the idealImgPerRow.
            
            if (idealImages.length === imgs.length) {
                //we have no more remaining images to fill the space, so we'll just use the calc height
                return targetHeight;
            }
            else {
                //we have additional images so we'll recurse and add 1 to the idealImgPerRow until it fits
                return getRowHeight(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow + 1, margin);
            }
        }
    }

    /** builds an image grid row */
    function buildRow(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin) {
        var currRowWidth = 0;
        var row = [];

        var calcRowHeight = getRowHeight(imgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin);

        var sizes = [];
        for (var i = 0; i < imgs.length; i++) {            
            var scaledWidth = getScaledWidth(imgs[i], calcRowHeight, maxRowWidth);
            if (currRowWidth + scaledWidth <= maxRowWidth) {
                currRowWidth += scaledWidth;
                sizes.push({ width: scaledWidth, height: calcRowHeight });                
                row.push(imgs[i]);
            }
            else {
                //the max width has been reached
                break;
            }
        }
        
        //loop through the images for the row and apply the styles
        for (var j = 0; j < row.length; j++) {
            var bottomMargin = margin;
            //make the margin 0 for the last one
            if (j === (row.length - 1)) {
                margin = 0;
            }
            setImageStyle(row[j], sizes[j].width, sizes[j].height, margin, bottomMargin);
        }

        return row;
    }

    /** Returns the maximum image scaling height for the current image collection */
    function getMaxScaleableHeight(imgs, maxRowHeight) {

        var smallestHeight = _.min(imgs, function (item) { return item.originalHeight; }).originalHeight;

        //adjust the smallestHeight if it is larger than the static max row height
        if (smallestHeight > maxRowHeight) {
            smallestHeight = maxRowHeight;
        }
        return smallestHeight;
    }

    /** Creates the image grid with calculated widths/heights for images to fill the grid nicely */
    function buildGrid(images, maxRowWidth, maxRowHeight, startingIndex, minDisplayHeight, idealImgPerRow, margin) {
        
        var rows = [];
        var imagesProcessed = 0;
        
        //first fill in all of the original image sizes and URLs
        for (var i = startingIndex; i < images.length; i++) {
            setImageUrl(images[i]);
            setOriginalSize(images[i], maxRowHeight);
        }

        while ((imagesProcessed + startingIndex) < images.length) {
            //get the maxHeight for the current un-processed images
            var currImgs = images.slice(imagesProcessed);
            
            //build the row
            var row = buildRow(currImgs, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin);
            if (row.length > 0) {
                rows.push(row);
                imagesProcessed += row.length;
            }
            else {
                //if there was nothing processed, exit
                break;
            }
        }

        return rows;
    }
    
    return {
        restrict: 'E',
        replace: true,
        require: '?ngModel',
        terminate: true,
        templateUrl: 'views/directives/html/umb-photo-folder.html',
        link: function (scope, element, attrs, ngModel) {
            
            ngModel.$render = function () {
                if (ngModel.$modelValue) {

                    $timeout(function () {
                        var photos = ngModel.$modelValue;

                        scope.clickHandler = scope.$eval(element.attr('on-click'));

                        //todo: this doesn't do anything
                        var imagesOnly = element.attr('imagesOnly');
                        
                        var margin = element.attr('border') ? parseInt(element.attr('border'), 10) : 5;                        
                        var startingIndex = element.attr('baseline') ? parseInt(element.attr('baseline'), 10) : 0;
                        var minWidth = element.attr('min-width') ? parseInt(element.attr('min-width'), 10) : 420;
                        var minHeight = element.attr('min-height') ? parseInt(element.attr('min-height'), 10) : 100;
                        var maxHeight = element.attr('max-height') ? parseInt(element.attr('max-height'), 10) : 300;
                        var idealImgPerRow = element.attr('ideal-items-per-row') ? parseInt(element.attr('ideal-items-per-row'), 10) : 5;

                        var fixedRowWidth = Math.max(element.width(), minWidth);

                        scope.rows = buildGrid(photos, fixedRowWidth, maxHeight, startingIndex, minHeight, idealImgPerRow, margin);
                        
                        if (attrs.filterBy) {
                            scope.$watch(attrs.filterBy, function (newVal, oldVal) {
                                if (newVal !== oldVal) {
                                    var p = $filter('filter')(photos, newVal, false);
                                    scope.baseline = 0;
                                    var m = buildGrid(p, fixedRowWidth, 400);
                                    scope.rows = m;
                                }
                            });
                        }

                    }, 500); //end timeout
                } //end if modelValue

            }; //end $render
        }
    };
});
