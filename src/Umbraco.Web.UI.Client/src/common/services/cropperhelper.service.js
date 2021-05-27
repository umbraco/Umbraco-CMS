/**
* @ngdoc service
* @name umbraco.services.cropperHelper
* @description A helper object used for dealing with image cropper data
**/
function cropperHelper(umbRequestHelper, $http) {
	var service = {

		/**
		* @ngdoc method
		* @name umbraco.services.cropperHelper#configuration
		* @methodOf umbraco.services.cropperHelper
		*
		* @description
		* Returns a collection of plugins available to the tinyMCE editor
		*
		*/
		configuration: function (mediaTypeAlias) {
			return umbRequestHelper.resourcePromise(
				$http.get(
					umbRequestHelper.getApiUrl(
						"imageCropperApiBaseUrl",
						"GetConfiguration",
						[{ mediaTypeAlias: mediaTypeAlias}])),
				'Failed to retrieve tinymce configuration');
		},


		//utill for getting either min/max aspect ratio to scale image after
		calculateAspectRatioFit : function(srcWidth, srcHeight, maxWidth, maxHeight, maximize) {
			var ratio = [maxWidth / srcWidth, maxHeight / srcHeight ];

			if(maximize){
				ratio = Math.max(ratio[0], ratio[1]);
			}else{
				ratio = Math.min(ratio[0], ratio[1]);
			}

			return { width:srcWidth*ratio, height:srcHeight*ratio, ratio: ratio};
		},

		//utill for scaling width / height given a ratio
		calculateSizeToRatio : function(srcWidth, srcHeight, ratio) {
			return { width:srcWidth*ratio, height:srcHeight*ratio, ratio: ratio};
		},

		scaleToMaxSize : function(srcWidth, srcHeight, maxWidth, maxHeight) {

            // fallback to maxHeight:
            maxHeight = maxHeight || maxWidth;

            // get smallest ratio, if ratio exceeds 1 we will not scale(hence we parse 1 as the maximum allowed ratio)
            var ratio = Math.min(maxWidth / srcWidth, maxHeight / srcHeight, 1);

			return {
                width: srcWidth * ratio,
                height:srcHeight * ratio
            };
		},

		//returns a ng-style object with top,left,width,height pixel measurements
		//expects {left,right,top,bottom} - {width,height}, {width,height}, int
		//offset is just to push the image position a number of pixels from top,left
		convertToStyle : function(coordinates, originalSize, viewPort, offset){

			var coordinates_px = service.coordinatesToPixels(coordinates, originalSize, offset);
			var _offset = offset || 0;

			var x = 1 - (coordinates.x1 + Math.abs(coordinates.x2));
			var left_of_x =  originalSize.width * x;
			var ratio = viewPort.width / left_of_x;

			var style = {
				position: "absolute",
				top:  -(coordinates_px.y1*ratio)+ _offset,
				left:  -(coordinates_px.x1* ratio)+ _offset,
				width: Math.floor(originalSize.width * ratio),
				height: Math.floor(originalSize.height * ratio),
				originalWidth: originalSize.width,
				originalHeight: originalSize.height,
				ratio: ratio
			};

			return style;
		},


		coordinatesToPixels : function(coordinates, originalSize, offset){

			var coordinates_px = {
				x1: Math.floor(coordinates.x1 * originalSize.width),
				y1: Math.floor(coordinates.y1 * originalSize.height),
				x2: Math.floor(coordinates.x2 * originalSize.width),
				y2: Math.floor(coordinates.y2 * originalSize.height)
			};

			return coordinates_px;
		},

		pixelsToCoordinates : function(image, width, height, offset){

			var x1_px = Math.abs(image.left-offset);
			var y1_px = Math.abs(image.top-offset);

			var x2_px = image.width - (x1_px + width);
			var y2_px = image.height - (y1_px + height);

			//crop coordinates in %
			var crop = {};
			crop.x1 = Math.max(x1_px / image.width, 0);
			crop.y1 = Math.max(y1_px / image.height, 0);
			crop.x2 = Math.max(x2_px / image.width, 0);
			crop.y2 = Math.max(y2_px / image.height, 0);

			return crop;
		},

		alignToCoordinates : function(image, center, viewport){

			var min_left = (image.width) - (viewport.width);
			var min_top =  (image.height) - (viewport.height);

			var c_top = -(center.top * image.height) + (viewport.height / 2);
			var c_left = -(center.left * image.width) + (viewport.width / 2);

			if(c_top < -min_top){
				c_top = -min_top;
			}
			if(c_top > 0){
				c_top = 0;
			}
			if(c_left < -min_left){
				c_left = -min_left;
			}
			if(c_left > 0){
				c_left = 0;
			}
			return {left: c_left, top: c_top};
		},


		syncElements : function(source, target){
				target.height(source.height());
				target.width(source.width());

				target.css({
					"top": source[0].offsetTop,
					"left": source[0].offsetLeft
				});
		}
	};

	return service;
}

angular.module('umbraco.services').factory('cropperHelper', cropperHelper);
