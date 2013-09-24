//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.Editors.MediaPickerController", 
	function($rootScope, $scope, dialogService, mediaResource, imageHelper, $log){
	
	
	$scope.images = [];
	$scope.ids = [];

	if($scope.model.value){
		$scope.ids = $scope.model.value.split(',');

		mediaResource.getByIds($scope.ids).then(function(medias){
			//img.media = media;
			$(medias).each(function(i, media){
				//shortcuts
				//TODO, do something better then this for searching
				var img = {};
				img.src = imageHelper.getImagePropertyValue({imageModel: media});
				img.thumbnail = imageHelper.getThumbnailFromPath(img.src);
				$scope.images.push(img);
			});
		});
	}

	$scope.remove = function(index){
		$scope.images.splice(index, 1);
		$scope.ids.splice(index, 1);
		$scope.sync();
	};

	$scope.add = function(){
		dialogService.mediaPicker({multipicker:true, callback: function(data){
			$(data.selection).each(function(i, media){
				//shortcuts
				//TODO, do something better then this for searching

				var img = {};
				img.id = media.id;
				img.src = imageHelper.getImagePropertyValue({imageModel: media});
				img.thumbnail = imageHelper.getThumbnailFromPath(img.src);
				$scope.images.push(img);
				$scope.ids.push(img.id);
			});

			$scope.sync();
		}});
	};

	$scope.sync = function(){
		$scope.model.value = $scope.ids.join();
	};

});