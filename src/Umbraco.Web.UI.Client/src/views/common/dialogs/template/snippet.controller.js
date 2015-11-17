angular.module("umbraco").controller('Umbraco.Dialogs.Template.SnippetController',
		function($scope) {
		    $scope.type = $scope.dialogOptions.type;
		    $scope.section = {
                name: "",
                required: false
		    };
		});