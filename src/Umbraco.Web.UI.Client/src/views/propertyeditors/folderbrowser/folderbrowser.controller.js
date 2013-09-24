angular.module("umbraco")
.directive("umbUploadPreview",function($parse){
        return {
            link: function(scope, element, attr, ctrl) {
               var fn = $parse(attr.umbUploadPreview),
                                   file = fn(scope);
                if (file.preview) {
                    element.append(file.preview);
               }
            }
        };
})
.controller("Umbraco.Editors.FolderBrowserController",
    function ($rootScope, $scope, assetsService, $routeParams, $timeout, $element, umbRequestHelper, mediaResource, imageHelper) {
        var dialogOptions = $scope.$parent.dialogOptions;

        $scope.minWidth = 480;
        $scope.minHeight = 200;
        $scope.lastWidth = 0;
        $scope.baseline = 0;

        $scope.filesUploading = [];

        $scope.options = {
            url: umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostAddFile"),
            autoUpload: true,
            disableImageResize: /Android(?!.*Chrome)|Opera/
            .test(window.navigator.userAgent),
            previewMaxWidth: 100,
            previewMaxHeight: 100,
            previewCrop: true,
            formData:{
                currentFolder: $routeParams.id
            }
        };

        $scope.loadChildren = function(id){
            mediaResource.getChildren(id)
                .then(function(data) {
                    $scope.images = data;
                    //update the thumbnail property
                    _.each($scope.images, function(img) {
                        //img.thumbnail = imageHelper.getThumbnail({ imageModel: img, scope: $scope });
                    });

                    // prettify the look and feel
                    $scope.lastWidth = Math.max($element.find(".umb-photo-folder-max-width").width(), $scope.minWidth);                
                    $scope.baseLine = 0;
                    $scope.processPhotos($scope.images);
                });    
        };

        $scope.$on('fileuploadstop', function(event, files){
            $scope.loadChildren($scope.options.formData.currentFolder);
            $scope.queue = [];
            $scope.filesUploading = [];
        });

        $scope.$on('fileuploadprocessalways', function(e,data) {
            var i;
            console.log('processing');

            $scope.$apply(function() {
                $scope.filesUploading.push(data.files[data.index]);
            });
        })


        // All these sit-ups are to add dropzone area and make sure it gets removed if dragging is aborted! 
        $scope.$on('fileuploaddragover', function(event, files) {
            if (!$scope.dragClearTimeout) {
                $scope.$apply(function() {
                    $scope.dropping = true;
                });
            } else {
                $timeout.cancel($scope.dragClearTimeout);
            }
            $scope.dragClearTimeout = $timeout(function () {
                $scope.dropping = null;
                $scope.dragClearTimeout = null;
            }, 300);
        })
        
        //init load
        $scope.loadChildren($routeParams.id);

        $scope.processPhotos = function(photos) {
            // divs to contain the images
            var d = $element.find(".umb-photo-folder");
            baseLine = $scope.baseLine;
            minWidth = $scope.minWidth;
            minHeight = $scope.minHeight;
            lastWidth = $scope.lastWidth;

            if( baseLine === 0 ) {
                d.empty();
            }
            
            // get row width - this is fixed.
            var w = lastWidth;
            
            // initial height - effectively the maximum height +/- 10%;
            var h = Math.max(minHeight,Math.floor(w / 5));
            // margin width
            var border = 5;

            // store relative widths of all images (scaled to match estimate height above)
            var ws = [];
            $.each(photos, function(key, val) {
                val.width_n = $.grep(val.properties, function(val, index) {return (val.alias === "umbracoWidth");})[0].value;
                val.height_n = $.grep(val.properties, function(val, index) {return (val.alias === "umbracoHeight");})[0].value;
                val.url_n = imageHelper.getThumbnail({ imageModel: val, scope: $scope });
                var wt = parseInt(val.width_n, 10);
                var ht = parseInt(val.height_n, 10);
                if( ht != h ) { wt = Math.floor(wt * (h / ht)); }
                ws.push(wt);
            });


            var rowNum = 0;
            var limit = photos.length;
            
            while(baseLine < limit)
            {
                rowNum++;
                // number of images appearing in this row
                var c = 0; 
                // total width of images in this row - including margins
                var tw = 0;
                
                // calculate width of images and number of images to view in this row.
                while( (tw * 1.1 < w) && (baseLine + c < limit))
                {
                    tw += ws[baseLine + c++] + border * 2;
                }

                // skip the last row
//                if( baseLine + c >= limit ) return;
                
                var d_row = $("<div/>", {"class" : "picrow"});
                d.append(d_row);
                

                // Ratio of actual width of row to total width of images to be used.
                var r = w / tw; 
                
                // image number being processed
                var i = 0;
                // reset total width to be total width of processed images
                tw = 0;
                // new height is not original height * ratio
                var ht = Math.floor(h * r);
                d_row.height(ht + border * 2);
                d_row.width(lastWidth);

                while( i < c )
                {
                    var photo = photos[baseLine + i];
                    // Calculate new width based on ratio
                    var wt = Math.floor(ws[baseLine + i] * r);
                    // add to total width with margins
                    tw += wt + border * 2;
                  
                    // Create image, set src, width, height and margin
                    var purl = photo.url_n;
                  
                    var img = $(
                      '<img/>', 
                      {
                          "class": "umb-photo", 
                          src: purl, 
                          width: wt, 
                          height: ht,
                      }).css("margin", border + "px").css("cursor", "pointer");
                    img.data("name", photo.name);
                    img.data("author", photo.owner.name);
                    img.data("date", photo.updateDate);
                    img.click(function() {location.href="#/media/media/edit/" + photo.id;});
                    d_row.append(img);

                    i++;
                }
                
                // set row height to actual height + margins
                baseLine += c;

                // if total width is slightly smaller than 
                // actual div width then add 1 to each 
                // photo width till they match
                i = 0;
                while( tw < w )
                {
                    var img1 = d_row.find("img:nth-child(" + (i + 1) + ")");
                    img1.width(img1.width() + 1);
                    i = (i + 1) % c;
                    tw++;
                }
                // if total width is slightly bigger than 
                // actual div width then subtract 1 from each 
                // photo width till they match
                i = 0;
                while( tw > w )
                {
                    var img2 = d_row.find("img:nth-child(" + (i + 1) + ")");
                    img2.width(img2.width() - 1);
                    i = (i + 1) % c;
                    tw--;
                }
            }

        };
    }
);