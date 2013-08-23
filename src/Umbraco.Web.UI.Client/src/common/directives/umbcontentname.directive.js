/**
* @ngdoc directive
* @name umbraco.directives.directive:umbContentName 
* @restrict E
* @function
* @description 
* Used by editors that require naming an entity. Shows a textbox/headline with a required validator within it's own form.
**/
angular.module("umbraco.directives")
	.directive('umbContentName', function ($timeout) {
	    return {
	        require: "ngModel",
			restrict: 'E',
			replace: true,
			templateUrl: 'views/directives/umb-content-name.html',
			scope: {
			    placeholder: '@placeholder',
			    model: '=ngModel'
			},
			link: function(scope, element, attrs, modelCtrl) {

				var input = $(element).find('input');
				var h1 = $(element).find('h1');
				input.hide();

				
				input.on("blur", function () {
				    //Don't hide the input field if there is no value in it
				    var val = input.val() || "empty";
				    input.hide();

				    h1.text(val);
				    h1.show();
				});


				h1.on("click", function () {
				    h1.hide();
		            input.show().focus();
				});

				$timeout(function(){
						if(!scope.model){
							    h1.hide();
					            input.show().focus();
					        }    
				}, 500);	
			}
	    };
	});