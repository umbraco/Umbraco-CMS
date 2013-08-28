//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.Editors.MediaPickerController", 
	function($rootScope, $scope, dialogService, mediaResource, imageHelper, $log){
	// 
	//$( "#draggable" ).draggable();


	//saved value contains a list of images with their coordinates a Dot coordinates
	//this will be $scope.model.value...
	var sampleData = [
		{id: 1143, coordinates: {x:123,y:345}, center: {x:123,y:12}},
		{id: 1144, coordinates: {x:123,y:345}, center: {x:123,y:12}},
		{id: 1145, coordinates: {x:123,y:345}, center: {x:123,y:12}},
	];

	$scope.images = sampleData;
	$($scope.images).each(function(i,img){
		mediaResource.getById(img.id).then(function(media){
			//img.media = media;

			//shortcuts
			//TODO, do something better then this for searching
			img.src = imageHelper.getImagePropertyVaue({imageModel: media});
			img.thumbnail = imageHelper.getThumbnailFromPath(img.src); 
		});
	});

 	//List of crops with name and size			
 	$scope.config = {
 		crops: [
	 		{name: "default", x:300,y:400},
	 		{name: "header", x:23,y:40},
	 		{name: "tiny", x:10,y:10}
 		]};


 		$scope.openMediaPicker =function(value){
 			var d = dialogService.mediaPicker({scope: $scope, callback: populate});
 		};

 		$scope.crop = function(image){
 			$scope.currentImage = image;
 		};

 		function populate(data){
 			$scope.model.value = data.selection;
 		}
 	});
