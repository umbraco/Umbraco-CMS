/**
* @ngdoc directive
* @name umbraco.directives.directive:umbPanel
* @restrict E
**/
angular.module("umbraco.directives.html")
	.directive('umbPanel', function($timeout, $log){
		return {
			restrict: 'E',
			replace: true,
			transclude: 'true',
			templateUrl: 'views/directives/html/umb-panel.html',
			link: function (scope, el, attrs) {
				
				
				function _setClass(bar, resize){
					
					bar = $(bar);

					//no need to process
					if(resize){
						bar.removeClass("umb-bottom-bar");
					}	

					//already positioned
					if(bar.hasClass("umb-bottom-bar")){
						return;
					}

					var offset = bar.offset();
					if(offset){
						var bottom = bar.offset().top + bar.height();
			            if(bottom > $(window).height()){
							bar.addClass("umb-bottom-bar");
							$(".tab-content .active").addClass("with-buttons");
						}else{
							bar.removeClass("umb-bottom-bar");
							$(".tab-content .active").removeClass("with-buttons");
						}	
					}
				}
				

				//initial loading
				$timeout(function(){	
					var bar = $(".tab-content .active .umb-tab-buttons")[0] || $(".tab-content .umb-tab-buttons")[0];
					var winHeight = $(window).height();

					scope.$watch(function () {
						if(!bar){
							bar = $(".tab-content .active .umb-tab-buttons")[0] || $(".tab-content .umb-tab-buttons")[0];
						}
						
						var bottom = bar.offsetTop + bar.offsetHeight;
						return bottom > winHeight;
					}, function(val) {
						_setClass(bar);
					});


					$(window).bind("resize", function () {
					  _setClass(bar, true);
					  winHeight = $(window).height();
					});

					$('a[data-toggle="tab"]').on('shown', function (e) {
						bar = $(".tab-content .active .umb-tab-buttons")[0];
						_setClass(bar);	
					});

				}, 1000, false);
			}
		};
	});