angular.module("umbraco")
.controller("Umbraco.Editors.ContentEditController",
	function ($scope, $routeParams, contentResource, notificationsService) {

	    if ($routeParams.create) {

	        contentResource.getContentScaffold($routeParams.id, $routeParams.doctype)
	            .then(function (data) {
	                $scope.content = data;
	            });
	    }
	    else {
	        contentResource.getById($routeParams.id)
	            .then(function (data) {
	                $scope.content = data;
	            });
	    }

	    $scope.files = [];
	    $scope.addFiles = function (propertyId, files) {
	        //this will clear the files for the current property and then add the new ones for the current property
	        $scope.files = _.reject($scope.files, function (item) {
	            return item.id == propertyId;
	        });
	        for (var i = 0; i < files.length; i++) {
	            //save the file object to the scope's files collection
	            $scope.files.push({ id: propertyId, file: files[i] });
	        }
	    };

	    $scope.saveAndPublish = function (cnt) {
	        cnt.publishDate = new Date();
	        contentResource.publishContent(cnt, $routeParams.create, $scope.files);
	        notificationsService.success("Published", "Content has been saved and published");
	    };

	    $scope.save = function (cnt) {
	        cnt.updateDate = new Date();
	        contentResource.saveContent(cnt, $routeParams.create, $scope.files);
	        notificationsService.success("Saved", "Content has been saved");
	    };
	});