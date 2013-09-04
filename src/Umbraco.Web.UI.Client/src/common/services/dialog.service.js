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
 *      //data contains whatever the dialog has selected / attached
 *		}			
 * </pre> 
 */

angular.module('umbraco.services')
.factory('dialogService', function ($rootScope, $compile, $http, $timeout, $q, $templateCache, $log) {

       var dialogs = [];
       
       /** Internal method that removes all dialogs */
       function removeAllDialogs(args) {
           for (var i = 0; i < dialogs.length; i++) {
               var dialog = dialogs[i];
               removeDialog(dialog, args);
               dialogs.splice(i, 1);
           }
       }

       /** Internal method that handles closing a specific dialog */
       function removeDialog(dialog, args) {

           //if there's arguments passed in then check if there's a callback registered in the current modal then call it.
           //this occurs when the "closeDialogs" event is triggered with arguments.

           if (args && dialog.data("modalCb") != null && angular.isFunction(dialog.data("modalCb"))) {
               var cb = dialog.data("modalCb");
               cb.apply(dialog, [args]);
           }

           dialog.modal("hide");

           $timeout(function () {
               dialog.remove();
           }, 250);
       }

       /** Internal method that handles opening all dialogs */
       function openDialog(options) {

     
           if (!options) {
               options = {};
           }
           //configation and defaults
           var scope = options.scope || $rootScope.$new(),
               container = options.container || $("body"),
               animationClass = options.animation || "fade",
               modalClass = options.modalClass || "umb-modal",
               width = options.width || "100%",
               templateUrl = options.template || "views/common/notfound.html";

           //if a callback is available
           var callback = options.callback;

           //Modal dom obj and unique id
           var $modal = $('<div data-backdrop="false"></div>');
           var id = templateUrl.replace('.html', '').replace('.aspx', '').replace(/[\/|\.|:\&\?\=]/g, "-") + '-' + scope.$id;
           
           if(options.inline){
              animationClass = "";
              modalClass = "";
           }else{
               $modal.addClass("modal");
               $modal.addClass("hide");
           }

           //set the id and add classes
           $modal
               .attr('id', id)
               .addClass(animationClass)
               .addClass(modalClass);

           //push the modal into the global modal collection
           //we halt the .push because a link click will trigger a closeAll right away
           $timeout(function () {
               dialogs.push($modal);
           }, 250);
           
           //if iframe is enabled, inject that instead of a template
           if (options.iframe) {
               var html = $("<iframe auto-scale='0' src='" + templateUrl + "' style='width: 100%; height: 100%;'></iframe>");

               $modal.html(html);
               //append to body or whatever element is passed in as options.containerElement
               container.append($modal);

               // Compile modal content
               $timeout(function () {
                   $compile($modal)(scope);
               });

               $modal.css("width", width);

               //Autoshow	
               if (options.show) {
                   $modal.modal('show');
               }

               //store the callback in the modal jquery data
               $modal.data("modalCb", callback);

               return $modal;
           }
           else {
               
               //We need to load the template with an httpget and once it's loaded we'll compile and assign the result to the container
               // object. However since the result could be a promise or just data we need to use a $q.when. We still need to return the 
               // $modal object so we'll actually return the modal object synchronously without waiting for the promise. Otherwise this openDialog
               // method will always need to return a promise which gets nasty because of promises in promises plus the result just needs a reference
               // to the $modal object which will not change (only it's contents will change).
               $q.when($templateCache.get(templateUrl) || $http.get(templateUrl, { cache: true }).then(function(res) { return res.data; }))
                   .then(function onSuccess(template) {

                       // Build modal object
                       $modal.html(template);

                       //append to body or other container element	
                       container.append($modal);

                       //store the callback in the modal jquery data
                       $modal.data("modalCb", callback);

                       // Compile modal content
                       $timeout(function() {
                           $compile($modal)(scope);
                       });

                       scope.dialogOptions = options;

                       //Scope to handle data from the modal form
                       scope.dialogData = {};
                       scope.dialogData.selection = [];

                       // Provide scope display functions
                       //this passes the modal to the current scope
                       scope.$modal = function(name) {
                           $modal.modal(name);
                       };

                       scope.hide = function() {
                           $modal.modal('hide');

                           $modal.remove();
                           $("#" + $modal.attr("id")).remove();
                       };

                       scope.show = function() {
                           $modal.modal('show');
                       };

                       scope.submit = function(data) {
                           if (callback) {
                               callback(data);
                           }

                           $modal.modal('hide');

                           $modal.remove();
                           $("#" + $modal.attr("id")).remove();
                       };

                       scope.select = function(item) {
                          var i = scope.dialogData.selection.indexOf(item);
                           if (i < 0) {
                               scope.dialogData.selection.push(item);
                           }else{
                              scope.dialogData.selection.splice(i, 1);
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
                       if (options.show) {
                           $modal.modal('show');
                       }
                   });
               
               //Return the modal object outside of the promise!
               return $modal;
           }
       }

       /** Handles the closeDialogs event */
       $rootScope.$on("closeDialogs", function (evt, args) {
           removeAllDialogs(args);
       });

       return {
           /**
            * @ngdoc method
            * @name umbraco.services.dialogService#open
            * @methodOf umbraco.services.dialogService
            *
            * @description
            * Opens a modal rendering a given template url.
            *
            * @param {Object} options rendering options
            * @param {DomElement} options.container the DOM element to inject the modal into, by default set to body
            * @param {Function} options.callback function called when the modal is submitted
            * @param {String} options.template the url of the template
            * @param {String} options.animation animation csss class, by default set to "fade"
            * @param {String} options.modalClass modal css class, by default "umb-modal"
            * @param {Bool} options.show show the modal instantly
            * @param {Object} options.scope scope to attach the modal to, by default rootScope.new()
            * @param {Bool} options.iframe load template in an iframe, only needed for serverside templates
            * @param {Int} options.width set a width on the modal, only needed for iframes
            * @param {Bool} options.inline strips the modal from any animation and wrappers, used when you want to inject a dialog into an existing container
            * @returns {Object} modal object
            */
           open: function (options) {               
               return openDialog(options);
           },
           
           /**
            * @ngdoc method
            * @name umbraco.services.dialogService#close
            * @methodOf umbraco.services.dialogService
            *
            * @description
            * Closes a specific dialog
            * @param {Object} dialog the dialog object to close
            * @param {Object} args if specified this object will be sent to any callbacks registered on the dialogs.
            */
           close: function (dialog, args) {
               removeDialog(dialog, args);
           },
           
           /**
            * @ngdoc method
            * @name umbraco.services.dialogService#closeAll
            * @methodOf umbraco.services.dialogService
            *
            * @description
            * Closes all dialogs
            * @param {Object} args if specified this object will be sent to any callbacks registered on the dialogs.
            */
           closeAll: function(args) {
               removeAllDialogs(args);
           },

           /**
            * @ngdoc method
            * @name umbraco.services.dialogService#mediaPicker
            * @methodOf umbraco.services.dialogService
            *
            * @description
            * Opens a media picker in a modal, the callback returns an array of selected media items
            * @param {Object} options mediapicker dialog options object
            * @param {$scope} options.scope dialog scope
            * @param {Function} options.callback callback function
            * @returns {Object} modal object
            */
           mediaPicker: function (options) {
               return openDialog({
                   scope: options.scope,  
                   callback: options.callback,
                   template: 'views/common/dialogs/mediaPicker.html',
                   show: true
               });
           },

           /**
            * @ngdoc method
            * @name umbraco.services.dialogService#contentPicker
            * @methodOf umbraco.services.dialogService
            *
            * @description
            * Opens a content picker tree in a modal, the callback returns an array of selected documents
            * @param {Object} options content picker dialog options object
            * @param {$scope} options.scope dialog scope
            * @param {$scope} options.multipicker should the picker return one or multiple items
            * @param {Function} options.callback callback function
            * @returns {Object} modal object
            */
           contentPicker: function (options) {
               return openDialog({
                   scope: options.scope,
                   callback: options.callback,
                   multipicker: options.multipicker,
                   template: 'views/common/dialogs/contentPicker.html',
                   show: true
               });
           },

           /**
            * @ngdoc method
            * @name umbraco.services.dialogService#macroPicker
            * @methodOf umbraco.services.dialogService
            *
            * @description
            * Opens a mcaro picker in a modal, the callback returns a object representing the macro and it's parameters
            * @param {Object} options mediapicker dialog options object
            * @param {$scope} options.scope dialog scope
            * @param {Function} options.callback callback function
            * @returns {Object} modal object
            */
           macroPicker: function (options) {
               return openDialog({
                   scope: options.scope,
                   callback: options.callback,
                   template: 'views/common/dialogs/macroPicker.html',
                   show: true
               });
           },

           /**
            * @ngdoc method
            * @name umbraco.services.dialogService#propertyDialog
            * @methodOf umbraco.services.dialogService
            *
            * @description
            * Opens a dialog with a chosen property editor in, a value can be passed to the modal, and this value is returned in the callback
            * @param {Object} options mediapicker dialog options object
            * @param {$scope} options.scope dialog scope
            * @param {Function} options.callback callback function
            * @param {String} editor editor to use to edit a given value and return on callback
            * @param {Object} value value sent to the property editor
            * @returns {Object} modal object
            */
           propertyDialog: function (options) {
               return openDialog({
                   scope: options.scope,
                   callback: options.callback,
                   template: 'views/common/dialogs/property.html',
                   show: true
               });
           },
           
           /**
           * @ngdoc method
           * @name umbraco.services.dialogService#ysodDialog
           * @methodOf umbraco.services.dialogService
           *
           * @description
           * Opens a dialog to show a custom YSOD
           */
           ysodDialog: function (ysodError) {

               var newScope = $rootScope.$new();
               newScope.error = ysodError;
               return openDialog({
                   modalClass: "umb-modal wide",
                   scope: newScope,
                   //callback: options.callback,
                   template: 'views/common/dialogs/ysod.html',
                   show: true
               });
           }
       };
   });