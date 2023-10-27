angular.module('umbraco.directives.validation')
.directive('valTriggerChange', function($sniffer) {
	return {
		link : function(scope, elem, attrs) {
			elem.on('click', function(){
				$(attrs.valTriggerChange).trigger($sniffer.hasEvent('input') ? 'input' : 'change');
			});
		},
		priority : 1	
	};
});