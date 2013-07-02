angular.module('umbraco.services')
.factory('dialogService', ['$rootScope', '$compile', '$http', '$timeout', '$q', '$templateCache', 
	function($rootScope, $compile, $http, $timeout, $q, $templateCache) {
	
	function _open(options){	
		if(!options){
			options = {};
		}

		var scope = options.scope || $rootScope.$new(),
			animationClass = options.animation || "fade",
			modalClass = options.modalClass || "umb-modalcolumn",
			templateUrl = options.template || "views/common/notfound.html";

		var callback = options.callback;
		return $q.when($templateCache.get(templateUrl) || $http.get(templateUrl, {cache: true}).then(function(res) { return res.data; }))
		.then(function onSuccess(template) {

					// Build modal object
					var id = templateUrl.replace('.html', '').replace(/[\/|\.|:]/g, "-") + '-' + scope.$id;
					var $modal = $('<div class="modal umb-modal hide" data-backdrop="false" tabindex="-1"></div>')
									.attr('id', id)
									.addClass(animationClass)
									.addClass(modalClass)
									.html(template);

					
							
					$('body').append($modal);

					// Compile modal content
					$timeout(function() {
						$compile($modal)(scope);
					});

					//Scope to handle data from the modal form
					scope.dialogData = {};
					scope.dialogData.selection = [];

					// Provide scope display functions
					scope.$modal = function(name) {
						$modal.modal(name);
					};
					
					scope.hide = function() {
					    $modal.modal('hide');
					    $('body').remove($modal);
					};

					scope.show = function() {
						$modal.modal('show');
					};

					scope.submit = function(data){
					    $modal.modal('hide');
					    $('body').remove($modal);

					    callback(data);
					};

					scope.select = function(item){
						if(scope.dialogData.selection.indexOf(item) < 0){
							scope.dialogData.selection.push(item);	
						}	
					};

					scope.dismiss = scope.hide;

					// Emit modal events
					angular.forEach(['show', 'shown', 'hide', 'hidden'], function(name) {
						$modal.on(name, function(ev) {
							scope.$emit('modal-' + name, ev);
						});
					});

					// Support autofocus attribute
					$modal.on('shown', function(event) {
						$('input[autofocus]', $modal).first().trigger('focus');
					});

					//Autoshow	
					if(options.show) {
						$modal.modal('show');
					}

					$rootScope.$on("closeDialogs", function(){
						$modal.modal("hide");
					});
					
					//Return the modal object	
					return $modal;
				});	
}

return{
	open: function(options){
		return _open(options);
	},
	mediaPicker: function(options){
		return _open({
			scope: options.scope, 
			callback: options.callback, 
			template: 'views/common/dialogs/mediaPicker.html', 
			show: true});
	},
	contentPicker: function(options){
		return _open({
			scope: options.scope, 
			callback: options.callback, 
			template: 'views/common/dialogs/contentPicker.html', 
			show: true});
	},
	macroPicker: function(options){
		return _open({
			scope: options.scope, 
			callback: options.callback, 
			template: 'views/common/dialogs/macroPicker.html', 
			show: true});
	},
	propertyDialog: function(options){
		return _open({
			scope: options.scope, 
			callback: options.callback, 
			template: 'views/common/dialogs/property.html', 
			show: true});
	},
	append : function(options){
		var scope = options.scope || $rootScope.$new(), 
		templateUrl = options.template;

		return $q.when($templateCache.get(templateUrl) || $http.get(templateUrl, {cache: true}).then(function(res) { return res.data; }))
		.then(function onSuccess(template) {

						// Compile modal content
						$timeout(function() {
							options.container.html(template);
							$compile(options.container)(scope);
						});

						return template;
					});
	}  
};
}]);	