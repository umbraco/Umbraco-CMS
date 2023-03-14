/**
 * @ngdoc service
 * @name umbraco.services.notificationsService
 *
 * @requires $rootScope
 * @requires $timeout
 * @requires angularHelper
 *
 * @description
 * Application-wide service for handling notifications, the umbraco application
 * maintains a single collection of notications, which the UI watches for changes.
 * By default when a notication is added, it is automaticly removed 7 seconds after
 * This can be changed on add()
 *
 * ##usage
 * To use, simply inject the notificationsService into any controller that needs it, and make
 * sure the umbraco.services module is accesible - which it should be by default.
 *
 * <pre>
 *		notificationsService.success("Document Published", "hooraaaay for you!");
 *      notificationsService.error("Document Failed", "booooh");
 * </pre>
 */
angular.module('umbraco.services')
.factory('notificationsService', function ($rootScope, $timeout, angularHelper) {

	var nArray = [];
	function setViewPath(view){
		if(view.indexOf('/') < 0)
		{
			view = "views/common/notifications/" + view;
		}

		if(view.indexOf('.html') < 0)
		{
			view = view + ".html";
		}
		return view;
	}

	var service = {

		/**
		* @ngdoc method
		* @name umbraco.services.notificationsService#add
		* @methodOf umbraco.services.notificationsService
		*
		* @description
		* Lower level api for adding notifcations, support more advanced options
		* @param {Object} item The notification item
		* @param {String} item.headline Short headline
		* @param {String} item.message longer text for the notication, trimmed after 200 characters, which can then be exanded
		* @param {String} item.type Notification type, can be: "success","warning","error" or "info"
		* @param {String} item.url url to open when notification is clicked
		* @param {String} item.target the target used together with `url`. Empty if not specified.
		* @param {String} item.view path to custom view to load into the notification box
		* @param {Array} item.actions Collection of button actions to append (label, func, cssClass)
		* @param {Boolean} item.sticky if set to true, the notification will not auto-close
		* @returns {Object} args notification object
		*/

		add: function(item) {
			angularHelper.safeApply($rootScope, function () {

				if(item.view){
					item.view = setViewPath(item.view);
					item.sticky = true;
					item.type = "form";
					item.headline = null;
				}


				//add a colon after the headline if there is a message as well
				if (item.message) {
					item.headline += ": ";
					if(item.message.length > 200) {
						item.sticky = true;
					}
				}

				//we need to ID the item, going by index isn't good enough because people can remove at different indexes
				// whenever they want. Plus once we remove one, then the next index will be different. The only way to
				// effectively remove an item is by an Id.
				item.id = String.CreateGuid();

				nArray.push(item);

				if(!item.sticky) {
					$timeout(
                        function() {
    						var found = _.find(nArray, function(i) {
        						return i.id === item.id;
        					});
        					if (found) {
        						var index = nArray.indexOf(found);
        						nArray.splice(index, 1);
        					}
    					}
                    , 10000);
				}

				return item;
			});

		},

		hasView : function(view){
			if(!view){
				return _.find(nArray, function(notification){ return notification.view;});
			}else{
				view = setViewPath(view).toLowerCase();
				return _.find(nArray, function(notification){ return notification.view.toLowerCase() === view;});
			}
		},
		addView: function(view, args){
			var item = {
				args: args,
				view: view
			};

			service.add(item);
		},

	    /**
		 * @ngdoc method
		 * @name umbraco.services.notificationsService#showNotification
		 * @methodOf umbraco.services.notificationsService
		 *
		 * @description
		 * Shows a notification based on the object passed in, normally used to render notifications sent back from the server
		 *
		 * @returns {Object} args notification object
		 */
        showNotification: function(args) {
            if (!args) {
                throw "args cannot be null";
            }
            if (args.type === undefined || args.type === null) {
                throw "args.type cannot be null";
            }
            if (!args.header) {
                throw "args.header cannot be null";
            }

            switch(args.type) {
                case 0:
                case 'Save':
                    //save
                    this.success(args.header, args.message);
                    break;
                case 1:
                case 'Info':
                    //info
                    this.info(args.header, args.message);
                    break;
                case 2:
                case 'Error':
                    //error
                    this.error(args.header, args.message);
                    break;
                case 3:
                case 'Success':
                    //success
                    this.success(args.header, args.message);
                    break;
                case 4:
                case 'Warning':
                    //warning
                    this.warning(args.header, args.message);
                    break;
            }
        },

	    /**
		 * @ngdoc method
		 * @name umbraco.services.notificationsService#success
		 * @methodOf umbraco.services.notificationsService
		 *
		 * @description
		 * Adds a green success notication to the notications collection
		 * This should be used when an operations *completes* without errors
		 *
		 * @param {String} headline Headline of the notification
		 * @param {String} message longer text for the notication, trimmed after 200 characters, which can then be exanded
		 * @returns {Object} notification object
		 */
	    success: function (headline, message) {
	        return service.add({ headline: headline, message: message, type: 'success', time: new Date() });
	    },
		/**
		 * @ngdoc method
		 * @name umbraco.services.notificationsService#error
		 * @methodOf umbraco.services.notificationsService
		 *
		 * @description
		 * Adds a red error notication to the notications collection
		 * This should be used when an operations *fails* and could not complete
		 *
		 * @param {String} headline Headline of the notification
		 * @param {String} message longer text for the notication, trimmed after 200 characters, which can then be exanded
		 * @returns {Object} notification object
		 */
	    error: function (headline, message) {
	        return service.add({ headline: headline, message: message, type: 'error', time: new Date() });
		},

		/**
		 * @ngdoc method
		 * @name umbraco.services.notificationsService#warning
		 * @methodOf umbraco.services.notificationsService
		 *
		 * @description
		 * Adds a yellow warning notication to the notications collection
		 * This should be used when an operations *completes* but something was not as expected
		 *
		 *
		 * @param {String} headline Headline of the notification
		 * @param {String} message longer text for the notication, trimmed after 200 characters, which can then be exanded
		 * @returns {Object} notification object
		 */
	    warning: function (headline, message) {
	        return service.add({ headline: headline, message: message, type: 'warning', time: new Date() });
		},

		/**
		 * @ngdoc method
		 * @name umbraco.services.notificationsService#warning
		 * @methodOf umbraco.services.notificationsService
		 *
		 * @description
		 * Adds a yellow warning notication to the notications collection
		 * This should be used when an operations *completes* but something was not as expected
		 *
		 *
		 * @param {String} headline Headline of the notification
		 * @param {String} message longer text for the notication, trimmed after 200 characters, which can then be exanded
		 * @returns {Object} notification object
		 */
	    info: function (headline, message) {
	        return service.add({ headline: headline, message: message, type: 'info', time: new Date() });
		},

		/**
		 * @ngdoc method
		 * @name umbraco.services.notificationsService#remove
		 * @methodOf umbraco.services.notificationsService
		 *
		 * @description
		 * Removes a notification from the notifcations collection at a given index
		 *
		 * @param {Int} index index where the notication should be removed from
		 */
		remove: function (index) {
            if (Utilities.isObject(index)){
				var i = nArray.indexOf(index);
				angularHelper.safeApply($rootScope, function() {
				    nArray.splice(i, 1);
				});
			}else{
				angularHelper.safeApply($rootScope, function() {
				    nArray.splice(index, 1);
				});
			}
		},

		/**
		 * @ngdoc method
		 * @name umbraco.services.notificationsService#removeAll
		 * @methodOf umbraco.services.notificationsService
		 *
		 * @description
		 * Removes all notifications from the notifcations collection
		 */
	    removeAll: function () {
	        angularHelper.safeApply($rootScope, function() {
	            nArray.length = 0;
	        });
		},

		/**
		 * @ngdoc property
		 * @name umbraco.services.notificationsService#current
		 * @propertyOf umbraco.services.notificationsService
		 *
		 * @description
		 * Returns an array of current notifications to display
		 *
		 * @returns {string} returns an array
		 */
		current: nArray,

		/**
		 * @ngdoc method
		 * @name umbraco.services.notificationsService#getCurrent
		 * @methodOf umbraco.services.notificationsService
		 *
		 * @description
		 * Method to return all notifications from the notifcations collection
		 */
		getCurrent: function(){
			return nArray;
		}
	};

	return service;
});
