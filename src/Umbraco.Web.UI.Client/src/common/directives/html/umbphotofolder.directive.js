/**
* @ngdoc directive
* @name umbraco.directives.directive:umbPhotoFolder
* @restrict E
**/
angular.module("umbraco.directives.html")
.directive('umbPhotoFolder', function ($compile, $log, $timeout, $filter, imageHelper, umbRequestHelper) {

    function renderCollection(scope, photos, fixedRowWidth) {
        
        var rows = [];

        // initial height - effectively the maximum height +/- 10%;
        var initialHeight = Math.max(scope.minHeight, Math.floor(fixedRowWidth / 5));

        // store relative widths of all images (scaled to match estimate height above)
        var ws = [];
        $.each(photos, function (key, val) {

            val.width_n = $.grep(val.properties, function (v, index) { return (v.alias === "umbracoWidth"); })[0];
            val.height_n = $.grep(val.properties, function (v, index) { return (v.alias === "umbracoHeight"); })[0];

            if (val.width_n && val.height_n) {
                var parsedWidth = parseInt(val.width_n.value, 10);
                var parsedHeight = parseInt(val.height_n.value, 10);

                //if the parsedHeight is less than the minHeight than set it to the minHeight
                //TODO: Should we set it to it's original in this case?
                if (parsedHeight >= scope.minHeight) {
                    if (parsedHeight !== initialHeight) {
                        parsedWidth = Math.floor(parsedWidth * (initialHeight / parsedHeight));
                    }

                    ws.push(parsedWidth);
                }
                else {
                    ws.push(scope.minHeight);
                }
                
            } else {
                //if its files or folders, we make them square
                ws.push(scope.minHeight);
            }
        });


        var rowNum = 0;
        var limit = photos.length;
        while (scope.baseline < limit) {
            rowNum++;
            // number of images appearing in this row
            var imgsPerRow = 0;
            // total width of images in this row - including margins
            var totalRowWidth = 0;

            // calculate width of images and number of images to view in this row.
            while ((totalRowWidth * 1.1 < fixedRowWidth) && (scope.baseline + imgsPerRow < limit)) {
                totalRowWidth += ws[scope.baseline + imgsPerRow++] + scope.border * 2;
            }

            // Ratio of actual width of row to total width of images to be used.
            var r = fixedRowWidth / totalRowWidth;
            // image number being processed
            var i = 0;
            // reset total width to be total width of processed images
            totalRowWidth = 0;

            // new height is not original height * ratio
            var ht = Math.floor(initialHeight * r);

            var row = {};
            row.photos = [];
            row.style = {};
            row.style = { "height": ht + scope.border * 2, "width": fixedRowWidth };
            rows.push(row);

            while (i < imgsPerRow) {
                var photo = photos[scope.baseline + i];
                // Calculate new width based on ratio
                var calcWidth = Math.floor(ws[scope.baseline + i] * r);
                // add to total width with margins
                totalRowWidth += calcWidth + scope.border * 2;

                //get the image property (if one exists)
                var imageProp = imageHelper.getImagePropertyValue({ imageModel: photo });
                if (!imageProp) {
                    //TODO: Do something better than this!!
                    photo.thumbnail = "none";
                }
                else {
                    
                    //get the proxy url for big thumbnails (this ensures one is always generated)
                    var thumbnailUrl = umbRequestHelper.getApiUrl(
                        "imagesApiBaseUrl",
                        "GetBigThumbnail",
                        [{ mediaId: photo.id }]);
                    photo.thumbnail = thumbnailUrl;
                }
                
                photo.style = { "width": calcWidth, "height": ht, "margin": scope.border + "px", "cursor": "pointer" };
                row.photos.push(photo);
                i++;
            }

            // set row height to actual height + margins
            scope.baseline += imgsPerRow;

            // if total width is slightly smaller than 
            // actual div width then add 1 to each 
            // photo width till they match
            
            /*i = 0;
            while (tw < w-1) {
                row.photos[i].style.width++;
                i = (i + 1) % c;
                tw++;
            }*/

            // if total width is slightly bigger than 
            // actual div width then subtract 1 from each 
            // photo width till they match
            i = 0;
            while (totalRowWidth > fixedRowWidth - 1) {
                row.photos[i].style.width--;
                i = (i + 1) % imgsPerRow;
                totalRowWidth--;
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

                        scope.imagesOnly = element.attr('imagesOnly');
                        scope.baseline = element.attr('baseline') ? parseInt(element.attr('baseline'), 10) : 0;
                        scope.minWidth = element.attr('min-width') ? parseInt(element.attr('min-width'), 10) : 420;
                        scope.minHeight = element.attr('min-height') ? parseInt(element.attr('min-height'), 10) : 200;
                        scope.border = element.attr('border') ? parseInt(element.attr('border'), 10) : 5;
                        scope.clickHandler = scope.$eval(element.attr('on-click'));
                        var fixedRowWidth = Math.max(element.width(), scope.minWidth);

                        scope.rows = renderCollection(scope, photos, fixedRowWidth);

                        if (attrs.filterBy) {
                            scope.$watch(attrs.filterBy, function (newVal, oldVal) {
                                if (newVal !== oldVal) {
                                    var p = $filter('filter')(photos, newVal, false);
                                    scope.baseline = 0;
                                    var m = renderCollection(scope, p);
                                    scope.rows = m;
                                }
                            });
                        }

                    }, 200); //end timeout
                } //end if modelValue

            }; //end $render
        }
    };
});
