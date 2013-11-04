/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTabView 
* @restrict E
**/
angular.module("umbraco.directives")
.directive('umbTabView', function($timeout, $log){
	return {
		restrict: 'E',
		replace: true,
		transclude: 'true',
		templateUrl: 'views/directives/umb-tab-view.html',

		link: function (scope, el, attrs) {
			
			function _setClass(resize){
				var bar = $(".tab-content .active .umb-tab-buttons");

				//no need to process
				if(resize){
					bar.removeClass("umb-bottom-bar");
				}	

				//already positioned
				if(bar.hasClass("umb-bottom-bar")){
					return;
				}

				var bottom = bar.offset().top + bar.height();
	            if(bottom > $(window).height()){
					bar.addClass("umb-bottom-bar");
					$(".tab-content .active").addClass("with-buttons");
				}else{
					bar.removeClass("umb-bottom-bar");
					$(".tab-content .active").removeClass("with-buttons");
				}
			}


			//initial loading
			$timeout(function(){
				$('a[data-toggle="tab"]').on('shown', function (e) {
					_setClass();	
				});

				_setClass();
			}, 500, false);
			
			$(window).bind("resize", function () {
			  _setClass(true);
			});	
		}
	};
});