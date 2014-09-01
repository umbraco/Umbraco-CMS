/**
 * @ngdoc controller
 * @name Umbraco.Editors.Template.EditController
 * @function
 * 
 * @description
 * The controller for editing templates
 */
function TemplateEditController($scope, $http, assetsService, templateResource, $routeParams, $log) {
    	
    	$scope.pageId = 1069;

    	$scope.rendering = false;
	    $scope.aceLoaded = function(_editor) {
	       // Options
	    };

	    var render = _.throttle(function(){
	    	templateResource.saveAndRender($scope.template.content, $scope.template.id, $scope.pageId).then(
	    		function(response){
		    		var iframe = document.getElementById('mockingbird');
		    		iframe = (iframe.contentWindow) ? iframe.contentWindow : (iframe.contentDocument.document) ? iframe.contentDocument.document : iframe.contentDocument;
		    		iframe.document.open();
		    		iframe.document.write(response);
		    		iframe.document.close();
		    		$scope.rendering = false;
	    		},
	    		function(response){
	    			$log.log(response);
	    			$scope.rendering = false;
	    		}
	    	);

	    }, 1000);

	    $scope.aceChanged = function(e) {
	       	if(!$scope.rendering){
	       		$scope.rendering = true;
	       		render();
	       	}
	     };


	     templateResource.getById($routeParams.id).then(function(template){
	     	$scope.template = template;
	     	if(!$scope.rendering){
	     		$scope.rendering = true;
	     		render();
	     	}
	     });
}

angular.module("umbraco").controller("Umbraco.Editors.Template.EditController", TemplateEditController);