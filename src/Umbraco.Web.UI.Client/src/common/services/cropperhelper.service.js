/**
* @ngdoc service
* @name umbraco.services.mediaHelper
* @description A helper object used for dealing with media items
**/
function cropperHelper(umbRequestHelper) {
	var service = {
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


		centerInsideViewPort : function(img, viewport){
			var image_width   = img.width(),
			image_height	= img.height(),
			mask_width		= viewport.width(),
			mask_height		= viewport.height(), 
			image_top		= img[0].offsetTop,
			image_left		= $image[0].offsetLeft,
			change			= true;

			var left = mask_width / 2 - image_width / 2,
				top = mask_height / 2 - image_height / 2;

			/*check for overflow
			if(image_left > 20 || image_left < mask_width - image_width){
				left = mask_width / 2 - image_width / 2;
				change = true;
			}*/
			
			if(change){
				img.css({
					'position': 'absolute',
					'left': left,
					'top': top
				});	
			}
			
		}


	};

	return service;
}

angular.module('umbraco.services').factory('cropperHelper', cropperHelper);