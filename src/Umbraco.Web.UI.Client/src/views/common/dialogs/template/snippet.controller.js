angular.module("umbraco").controller('Umbraco.Dialogs.Template.SnippetController',
		function($scope, $http, dialogService) {
		    $scope.type = $scope.dialogOptions.type;
		    $scope.section = {};
		    $scope.section.name = "";
		    $scope.section.required = false;          
		});