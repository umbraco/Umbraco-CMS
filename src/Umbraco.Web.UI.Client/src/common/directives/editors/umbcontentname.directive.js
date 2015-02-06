/**
* @ngdoc directive
* @name umbraco.directives.directive:umbContentName 
* @restrict E
* @function
* @description 
* Used by editors that require naming an entity. Shows a textbox/headline with a required validator within it's own form.
**/
angular.module("umbraco.directives")
	.directive('umbContentName', function ($timeout, localizationService) {
	    return {
	        require: "ngModel",
			restrict: 'E',
			replace: true,
			templateUrl: 'views/directives/umb-content-name.html',
			scope: {
			    placeholder: '@placeholder',
			    model: '=ngModel',
                ngDisabled: '='
			},
			link: function(scope, element, attrs, ngModel) {

				var inputElement = element.find("input");
				if(scope.placeholder && scope.placeholder[0] === "@"){
					localizationService.localize(scope.placeholder.substring(1))
						.then(function(value){
							scope.placeholder = value;	
						});
				}
			    
				var mX, mY, distance;

				function calculateDistance(elem, mouseX, mouseY) {

				    var cx = Math.max(Math.min(mouseX, elem.offset().left + elem.width()), elem.offset().left);
				    var cy = Math.max(Math.min(mouseY, elem.offset().top + elem.height()), elem.offset().top);
				    return Math.sqrt((mouseX - cx) * (mouseX - cx) + (mouseY - cy) * (mouseY - cy));
				}

				var mouseMoveDebounce = _.throttle(function (e) {
				    mX = e.pageX;
				    mY = e.pageY;
				    // not focused and not over element
				    if (!inputElement.is(":focus") && !inputElement.hasClass("ng-invalid")) {
				        // on page
				        if (mX >= inputElement.offset().left) {
				            distance = calculateDistance(inputElement, mX, mY);
				            if (distance <= 155) {

				                distance = 1 - (100 / 150 * distance / 100);
				                inputElement.css("border", "1px solid rgba(175,175,175, " + distance + ")");
				                inputElement.css("background-color", "rgba(255,255,255, " + distance + ")");
				            }
				        }

				    }

				}, 15);

				$(document).bind("mousemove", mouseMoveDebounce);

				$timeout(function(){
					if(!scope.model){
						scope.goEdit();
					}
				}, 100, false);
			
				scope.goEdit = function(){
					scope.editMode = true;

					$timeout(function () {					    
					    inputElement.focus();					    
					}, 100, false);
				};

				scope.exitEdit = function(){
					if(scope.model && scope.model !== ""){
						scope.editMode = false;	
					}
				};

			    //unbind doc event!
				scope.$on('$destroy', function () {
				    $(document).unbind("mousemove", mouseMoveDebounce);
				});
			}
	    };
	});