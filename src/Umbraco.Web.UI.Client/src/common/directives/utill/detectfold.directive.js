/**
* @ngdoc directive
* @name umbraco.directives.directive:umbPanel
* @restrict E
**/
angular.module("umbraco.directives.html")
	.directive('detectFold', function($timeout, $log){
		return {
			restrict: 'A',
			link: function (scope, el, attrs) {
				
				var state = false,
					parent = $(".umb-panel-body"),
					winHeight = $(window).height(),
					calculate = _.throttle(function(){
						if(el && el.is(":visible") && !el.hasClass("umb-bottom-bar")){
							//var parent = el.parent();
							var hasOverflow = parent.innerHeight() < parent[0].scrollHeight;
							//var belowFold = (el.offset().top + el.height()) > winHeight;
							if(hasOverflow){
								el.addClass("umb-bottom-bar");
							}
						}
						return state;
					}, 1000);

				scope.$watch(calculate, function(newVal, oldVal) {
					if(newVal !== oldVal){
						if(newVal){
							el.addClass("umb-bottom-bar");
						}else{
							el.removeClass("umb-bottom-bar");
						}	
					}
				});

				$(window).bind("resize", function () {
				   winHeight = $(window).height();
				   el.removeClass("umb-bottom-bar");
				   state = false;
				   calculate();
				});

				$('a[data-toggle="tab"]').on('shown', function (e) {
					calculate();
				});
			}
		};
	});