/**
 * @ngdoc service
 * @name umbraco.services.dialogService
 *
 * @requires $rootScope 
 * @requires $compile
 * @requires $http
 * @requires $log
 * @requires $q
 * @requires $templateCache
 *	
 * @description
 * Application-wide service for handling modals, overlays and dialogs
 * By default it injects the passed template url into a div to body of the document
 * And renders it, but does also support rendering items in an iframe, incase
 * serverside processing is needed, or its a non-angular page
 *
 * ##usage
 * To use, simply inject the dialogService into any controller that needs it, and make
 * sure the umbraco.services module is accesible - which it should be by default.
 *
 * <pre>
 *		var dialog = dialogService.open({template: 'path/to/page.html', show: true, callback: done});
 *		functon done(data){
 *			//The dialog has been submitted	
 *		}			
 * </pre> 
 */

angular.module('umbraco.services')
.factory('dialogService', ['$rootScope', '$compile', '$http', '$timeout', '$q', '$templateCache',
   function ($rootScope, $compile, $http, $timeout, $q, $templateCache) {
       var _dialogs = [];
       $rootScope.$on("closeDialogs", function () {
           for (var i = 0; i < _dialogs.length; i++) {
               var dialog = _dialogs[i];

               dialog.modal("hide");
               dialog.remove();
               $("#" + dialog.attr("id")).remove();
               _dialogs.splice(i, 1);
           }
       });

       //internal method that handles opening all dialogs
       function _open(options) {
           if (!options) {
               options = {};
           }
           //configation and defaults
           var scope = options.scope || $rootScope.$new(),
               container = options.container || $("body"),
               animationClass = options.animation || "fade",
               modalClass = options.modalClass || "umb-modal",
               templateUrl = options.template || "views/common/notfound.html";

           //if a callback is available
           var callback = options.callback;

           //Modal dom obj and unique id
           var $modal = $('<div class="hide" data-backdrop="false"></div>');
           var id = templateUrl.replace('.html', '').replace(/[\/|\.|:]/g, "-") + '-' + scope.$id;

           if(options.inline){
              animationClass = "";
              modalClass = "";
           }else{
            $modal.addClass("modal");
           }

           //set the id and add classes
           $modal
               .attr('id', id)
               .addClass(animationClass)
               .addClass(modalClass);

           

           //push the modal into the global modal collection
           _dialogs.push($modal);

           //if iframe is enabled, inject that instead of a template
           if (options.iframe) {
               var html = $("<iframe auto-scale='0' src='" + templateUrl + "' style='width: 100%; height: 100%;'></iframe>");

               $modal.html(html);
               //append to body or whatever element is passed in as options.containerElement
               container.append($modal);

               if (width) {
                   $modal.css("width", width);
               }

               //Autoshow	
               if (options.show) {
                   $modal.modal('show');
               }

               return $modal;
           } else {
               return $q.when($templateCache.get(templateUrl) || $http.get(templateUrl, { cache: true }).then(function (res) { return res.data; }))
                       .then(function onSuccess(template) {

                           // Build modal object
                           $modal
                               .html(template);

                           //append to body or other container element	
                           container.append($modal);

                           // Compile modal content
                           $timeout(function () {
                               $compile($modal)(scope);
                           });

                           //Scope to handle data from the modal form
                           scope.dialogData = {};
                           scope.dialogData.selection = [];

                           // Provide scope display functions
                           //this passes the modal to the current scope
                           scope.$modal = function (name) {
                               $modal.modal(name);
                           };

                           scope.hide = function () {
                               $modal.modal('hide');

                               $modal.remove();
                               $("#" + $modal.attr("id")).remove();
                           };

                           scope.show = function () {
                               $modal.modal('show');
                           };

                           scope.submit = function (data) {
                               if (callback) {
                                   callback(data);
                               }

                               $modal.modal('hide');

                               $modal.remove();
                               $("#" + $modal.attr("id")).remove();
                           };

                           scope.select = function (item) {
                               if (scope.dialogData.selection.indexOf(item) < 0) {
                                   scope.dialogData.selection.push(item);
                               }
                           };

                           scope.dismiss = scope.hide;

                           // Emit modal events
                           angular.forEach(['show', 'shown', 'hide', 'hidden'], function (name) {
                               $modal.on(name, function (ev) {
                                   scope.$emit('modal-' + name, ev);
                               });
                           });

                           // Support autofocus attribute
                           $modal.on('shown', function (event) {
                               $('input[autofocus]', $modal).first().trigger('focus');
                           });

                           //Autoshow	
                           if (options.show) {
                               $modal.modal('show');
                           }

                           //Return the modal object	
                           return $modal;
                       });
           }


       }

       return {
           /**
            * @ngdoc method
            * @name umbraco.services.dialogService#open
            * @methodOf umbraco.services.dialogService
            *
            * @description
            * Opens a modal rendering a given template url
            * @param {Object} options rendering options
            * @param {DomElement} options.container the DOM element to inject the modal into, by default set to body
            * @param {Function} options.callback function called when the modal is submitted
            * @param {String} options.template the url of the template
            * @param {Bool} options.show show the modal instantly
            * @param {Object} options.scope scope to attach the modal to, by default rootScope.new()
            * @param {Bool} options.iframe load template in an iframe, only needed for serverside templates
            * @param {Int} options.width set a width on the modal, only needed for iframes
            */
           open: function (options) {
               return _open(options);
           },
           mediaPicker: function (options) {
               return _open({
                   scope: options.scope,
                   callback: options.callback,
                   template: 'views/common/dialogs/mediaPicker.html',
                   show: true
               });
           },
           contentPicker: function (options) {
               return _open({
                   scope: options.scope,
                   callback: options.callback,
                   template: 'views/common/dialogs/contentPicker.html',
                   show: true
               });
           },
           macroPicker: function (options) {
               return _open({
                   scope: options.scope,
                   callback: options.callback,
                   template: 'views/common/dialogs/macroPicker.html',
                   show: true
               });
           },
           propertyDialog: function (options) {
               return _open({
                   scope: options.scope,
                   callback: options.callback,
                   template: 'views/common/dialogs/property.html',
                   show: true
               });
           },

           //deprecated
           append: function (options) {

               return _open(options);

               /*
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
                           });*/
           }
       };
   }]);