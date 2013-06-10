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


	    $scope.saveAndPublish = function (cnt) {
	        cnt.publishDate = new Date();
	        contentResource.publishContent(cnt);
	        notificationsService.success("Published", "Content has been saved and published");
	    };

	    $scope.save = function (cnt) {
	        cnt.updateDate = new Date();
	        contentResource.saveContent(cnt);
	        notificationsService.success("Saved", "Content has been saved");
	    };
	});