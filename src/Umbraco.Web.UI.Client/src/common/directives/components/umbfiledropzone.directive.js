/**
* @ngdoc directive
* @name umbraco.directives.directive:umbContentName 
* @restrict E
* @function
* @description 
* Used by editors that require naming an entity. Shows a textbox/headline with a required validator within it's own form.
**/

/*
TODO
.directive("umbFileDrop", function ($timeout, $upload, localizationService, umbRequestHelper){

	return{
		restrict: "A",
		link: function(scope, element, attrs){

			//load in the options model


		}
	}
})
*/

angular.module("umbraco.directives")

.directive('umbFileDropzone', function ($timeout, $upload, localizationService, umbRequestHelper) {
	return {

		restrict: 'E',
		replace: true,

		templateUrl: 'views/components/umb-file-dropzone.html',

		scope: {
			parentId: '@',
			contentTypeAlias: '@',
			propertyAlias: '@',
			accept: '@',

			compact: '@',
			hideDropzone: '@',

			filesQueued: '=',
			handleFile: '=',
			filesUploaded: '='
		},

		link: function(scope, element, attrs) {

			scope.queue = [];
			scope.done = [];
			scope.currentFile = undefined;


			function _filesQueued(files, event){
				
				//Push into the queue
				angular.forEach(files, function(file){
					scope.queue.push(file);
				});

				//when queue is done, kick the uploader
				if(!scope.working){
					_processQueueItem();
				}				
			}


			function _processQueueItem(){
				if(scope.queue.length > 0){
					scope.currentFile = scope.queue.shift();
					_upload(scope.currentFile);
				}else if(scope.done.length > 0){

					if(scope.filesUploaded){
						//queue is empty, trigger the done action
						scope.filesUploaded(scope.done);
					}

					//auto-clear the done queue after 3 secs
					var currentLength = scope.done.length;
					$timeout(function(){
						scope.done.splice(0, currentLength);
					}, 3000);
				}
			}

			function _upload(file) {

				scope.propertyAlias = scope.propertyAlias ? scope.propertyAlias : "umbracoFile";
				scope.contentTypeAlias = scope.contentTypeAlias ? scope.contentTypeAlias : "Image"; 

				$upload.upload({
					url: umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostAddFile"),
					fields: {
						'currentFolder': scope.parentId, 
						'contentTypeAlias': scope.contentTypeAlias, 
						'propertyAlias': scope.propertyAlias
					},
					file: file
				}).progress(function (evt) {

					// calculate progress in percentage
					var progressPercentage = parseInt(100.0 * evt.loaded / evt.total, 10);

					// set percentage property on file
					file.uploadProgress = progressPercentage;

					// set uploading status on file
					file.uploadStatus = "uploading";

				}).success(function (data, status, headers, config) {

					// set done status on file
					file.uploadStatus = "done";

					// set date/time for when done - used for sorting
					file.doneDate = new Date();

					scope.done.push(file);
					scope.currentFile = undefined;

					//after processing, test if everthing is done
					_processQueueItem();

				}).error( function (evt) {
					file.uploadStatus = "error";

					//after processing, test if everthing is done
					scope.done.push(file);
					scope.currentFile = undefined;
					
					_processQueueItem();
				});
			}


			scope.handleFiles = function(files, event){
				if(scope.filesQueued){
					scope.filesQueued(files, event);
				}

				_filesQueued(files, event);
				
			};
			
			}


		};
	});