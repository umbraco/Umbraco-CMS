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

	function add(item) {	    
	    angularHelper.safeApply($rootScope, function () {
	        
	        //we need to ID the item, going by index isn't good enough because people can remove at different indexes 
	        // whenever they want. Plus once we remove one, then the next index will be different. The only way to 
	        // effectively remove an item is by an Id.
	        item.id = String.CreateGuid();
	        
	        nArray.push(item);
            
	        $timeout(function () {

	            var found = _.find(nArray, function(i) {
	                return i.id === item.id;
	            });
                if (found) {
                    var index = nArray.indexOf(found);
                    nArray.splice(index, 1);
                }
	            
	        }, 7000);

	        return item;
	    });
	}

	return {

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
                    //save
                    this.success(args.header, args.message);
                    break;
                case 1:
                    //info
                    this.success(args.header, args.message);
                    break;
                case 2:
                    //error
                    this.error(args.header, args.message);
                    break;
                case 3:
                    //success
                    this.success(args.header, args.message);
                    break;
                case 4:
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
	        return add({ headline: headline, message: message, type: 'success', time: new Date() });
	        
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
	        return add({ headline: headline, message: message, type: 'error', time: new Date() });
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
	        return add({ headline: headline, message: message, type: 'warning', time: new Date() });
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
	        angularHelper.safeApply($rootScope, function() {
	            nArray.splice(index, 1);
	        });
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
	            nArray = [];
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
});