/**
 * @ngdoc service
 * @name umbraco.services.historyService
 *
 * @requires $rootScope 
 * @requires $timeout
 * @requires angularHelper
 *	
 * @description
 * Service to handle the main application navigation history. Responsible for keeping track
 * of where a user navigates to, stores an icon, url and name in a collection, to make it easy
 * for the user to go back to a previous editor / action
 *
 * **Note:** only works with new angular-based editors, not legacy ones
 *
 * ##usage
 * To use, simply inject the historyService into any controller that needs it, and make
 * sure the umbraco.services module is accesible - which it should be by default.
 *
 * <pre>
 *      angular.module("umbraco").controller("my.controller". function(historyService){
 *         historyService.add({
 *								icon: "icon-class",
 *								name: "Editing 'articles',
 *								link: "/content/edit/1234"}
 *							);
 *      }); 
 * </pre> 
 */
angular.module('umbraco.services')
.factory('historyService', function ($rootScope, $timeout, angularHelper, eventsService) {

	var nArray = [];

	function add(item) {

	    if (!item) {
	        return null;
	    }

	    var listWithoutThisItem = _.reject(nArray, function(i) {
	        return i.link === item.link;
	    });

        //put it at the top and reassign
	    listWithoutThisItem.splice(0, 0, item);
	    nArray = listWithoutThisItem;
	    return nArray[0];

	}

	return {
		/**
		 * @ngdoc method
		 * @name umbraco.services.historyService#add
		 * @methodOf umbraco.services.historyService
		 *
		 * @description
		 * Adds a given history item to the users history collection.
		 *
		 * @param {Object} item the history item
		 * @param {String} item.icon icon css class for the list, ex: "icon-image", "icon-doc"
		 * @param {String} item.link route to the editor, ex: "/content/edit/1234"
		 * @param {String} item.name friendly name for the history listing
		 * @returns {Object} history item object
		 */
		add: function (item) {
			var icon = item.icon || "icon-file";
			angularHelper.safeApply($rootScope, function () {
			    var result = add({ name: item.name, icon: icon, link: item.link, time: new Date() });
			    eventsService.emit("historyService.add", {added: result, all: nArray});
			    return result;
			});
		},
		/**
		 * @ngdoc method
		 * @name umbraco.services.historyService#remove
		 * @methodOf umbraco.services.historyService
		 *
		 * @description
		 * Removes a history item from the users history collection, given an index to remove from.
		 *
		 * @param {Int} index index to remove item from
		 */
		remove: function (index) {
			angularHelper.safeApply($rootScope, function() {
			    var result = nArray.splice(index, 1);
			    eventsService.emit("historyService.remove", { removed: result, all: nArray });
			});
		},

		/**
		 * @ngdoc method
		 * @name umbraco.services.historyService#removeAll
		 * @methodOf umbraco.services.historyService
		 *
		 * @description
		 * Removes all history items from the users history collection
		 */
		removeAll: function () {
			angularHelper.safeApply($rootScope, function() {
			    nArray = [];
			    eventsService.emit("historyService.removeAll");
			});
		},

		/**
		 * @ngdoc method
		 * @name umbraco.services.historyService#getCurrent
		 * @methodOf umbraco.services.historyService
		 *
		 * @description
		 * Method to return the current history collection.
		 *
		 */
		getCurrent: function(){
			return nArray;
        },

        /**
		 * @ngdoc method
		 * @name umbraco.services.historyService#getLastAccessedItemForSection
		 * @methodOf umbraco.services.historyService
		 *
		 * @description
		 * Method to return the item that was last accessed in the given section
		 *
         * @param {string} sectionAlias Alias of the section to return the last accessed item for.
		 */
        getLastAccessedItemForSection: function (sectionAlias) {
            for (var i = 0, len = nArray.length; i < len; i++) {
                var item = nArray[i];
                if (item.link === sectionAlias || item.link.indexOf(sectionAlias + "/") === 0) {
                    return item;
                }
            }

            return null;
        }
	};
});
