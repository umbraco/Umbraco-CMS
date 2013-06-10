//used for the media picker dialog
angular.module("umbraco")
.controller("Umbraco.Dialogs.MediaPickerController",
	function ($scope, mediaResource) {	
	$scope.images = mediaResource.rootMedia();
});