/**
 * @ngdoc controller
 * @name Umbraco.Editors.ContentType.EditController
 * @function
 * 
 * @description
 * The controller for the content type editor
 */
function ContentTypeEditController($scope, $routeParams, $log, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, entityResource) {
    
    $scope.tabs = [];
    $scope.page = {};
    $scope.contentType = {tabs: [], name: "My content type", alias:"myType", icon:"icon-folder", allowedChildren: [], allowedTemplate: []};
    $scope.contentType.tabs = [
            {name: "Content", properties:[ {name: "test"}]},
            {name: "Generic Properties", properties:[]}
        ];


        
    $scope.dataTypesOptions ={
    	group: "properties",
    	onDropHandler: function(item, args){
    		args.sourceScope.move(args);
    	},
    	onReleaseHandler: function(item, args){
    		var a = args;
    	}
    };

    $scope.tabOptions ={
    	group: "tabs",
    	drop: false,
    	nested: true,
    	onDropHandler: function(item, args){
    		
    	},
    	onReleaseHandler: function(item, args){
    		
    	}
    };

    $scope.propertiesOptions ={
    	group: "properties",
    	onDropHandler: function(item, args){
    		//alert("dropped on properties");
			//args.targetScope.ngModel.$modelValue.push({name: "bong"});
    	},
    	onReleaseHandler: function(item, args){
    		//alert("released from properties");
			//args.targetScope.ngModel.$modelValue.push({name: "bong"});
    	},
    };


    $scope.omg = function(){
    	alert("wat");
    };   

    entityResource.getAll("Datatype").then(function(data){
        $scope.page.datatypes = data;
    });
}

angular.module("umbraco").controller("Umbraco.Editors.ContentType.EditController", ContentTypeEditController);