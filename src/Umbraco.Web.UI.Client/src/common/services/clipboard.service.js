/**
 * @ngdoc service
 * @name umbraco.services.clipboardService
 *
 * @requires notificationsService
 * @requires eventsService
 *
 * @description
 * Service to handle clipboard in general across the application. Responsible for handling the data both storing and retrive.
 * The service has a set way for defining a data-set by a entryType and alias, which later will be used to retrive the posible entries for a paste scenario.
 *
 */
function clipboardService(notificationsService, eventsService, localStorageService) {
    
    
    var STORAGE_KEY = "umbClipboardService";
    
    var retriveStorage = function() {
        if (localStorageService.isSupported === false) {
            return null;
        }
        var dataJSON;
        var dataString = localStorageService.get(STORAGE_KEY);
        if (dataString != null) {
            dataJSON = JSON.parse(dataString);
        }
        
        if(dataJSON == null) {
            dataJSON = new Object();
        }
        
        if(dataJSON.entries === undefined) {
            dataJSON.entries = [];
        }
        
        return dataJSON;
    }
    
    var saveStorage = function(storage) {
        var storageString = JSON.stringify(storage);
        
        try {
            var storageJSON = JSON.parse(storageString);
            localStorageService.set(STORAGE_KEY, storageString);
            
            eventsService.emit("clipboardService.storageUpdate");
            
            return true;
        } catch(e) {
            return false;
        }
        
        return false;
    }
    
    
    var service = {};
    
    /**
    * @ngdoc method
    * @name umbraco.services.clipboardService#copy
    * @methodOf umbraco.services.clipboardService
    *
    * @param type {string} umbraco A string defining the type of data to storing, example: 'elementType', 'contentNode'
    * @param alias {string} umbraco A string defining the alias of the data to store, example: 'product'
    * @param data {object} umbraco A object containing the properties to be saved.
    *
    * @description
    * Saves a single JS-object with a type and alias to the clipboard.
    */
    service.copy = function(type, alias, data) {
        
        var storage = retriveStorage();
        
        var shallowCloneData = Object.assign({}, data);// Notice only a shallow copy, since we dont need to deep copy. (that will happen when storing the data)
        delete shallowCloneData.key;
        delete shallowCloneData.$$hashKey;
        
        var key = data.key || data.$$hashKey || console.error("missing unique key for this content");
        
        // remove previous copies of this entry:
        storage.entries = storage.entries.filter(
            (entry) => {
                return entry.unique !== key;
            }
        );
        
        var entry = {unique:key, type:type, alias:alias, data:shallowCloneData};
        storage.entries.push(entry);
        
        if (saveStorage(storage) === true) {
            notificationsService.success("Clipboard", "Copied to clipboard.");
        } else {
            notificationsService.success("Clipboard", "Couldnt copy this data to clipboard.");
        }
        
    };
    
    
    /**
    * @ngdoc method
    * @name umbraco.services.supportsCopy#supported
    * @methodOf umbraco.services.clipboardService
    *
    * @description
    * Determins wether the current browser is able to performe its actions.
    */
    service.isSupported = function() {
        return localStorageService.isSupported;
    };
    
    /**
    * @ngdoc method
    * @name umbraco.services.supportsCopy#hasEntriesOfType
    * @methodOf umbraco.services.clipboardService
    *
    * @param type {string} umbraco A string defining the type of data test for.
    * @param aliases {string} umbraco A array of strings providing the alias of the data you want to test for.
    *
    * @description
    * Determines whether the current clipboard has entries that match a given type and one of the aliases.
    */
    service.hasEntriesOfType = function(type, aliases) {
        
        if(service.retriveEntriesOfType(type, aliases).length > 0) {
            return true;
        }
        
        return false;
    };
    
    /**
    * @ngdoc method
    * @name umbraco.services.supportsCopy#retriveEntriesOfType
    * @methodOf umbraco.services.clipboardService
    *
    * @param type {string} umbraco A string defining the type of data to recive.
    * @param aliases {string} umbraco A array of strings providing the alias of the data you want to recive.
    * 
    * @description
    * Returns an array of entries matching the given type and one of the provided aliases.
    */
    service.retriveEntriesOfType = function(type, aliases) {
        
        var storage = retriveStorage();
        
        // Find entries that are fulfilling the criteria for this nodeType and nodeTypesAliases.
        var filteretEntries = storage.entries.filter(
            (entry) => {
                return (entry.type === type && aliases.filter(alias => alias === entry.alias).length > 0);
            }
        );
        
        return filteretEntries;
    };
    
    /**
    * @ngdoc method
    * @name umbraco.services.supportsCopy#retriveEntriesOfType
    * @methodOf umbraco.services.clipboardService
    *
    * @param type {string} umbraco A string defining the type of data to recive.
    * @param aliases {string} umbraco A array of strings providing the alias of the data you want to recive.
    * 
    * @description
    * Returns an array of data of entries matching the given type and one of the provided aliases.
    */
    service.retriveDataOfType = function(type, aliases) {
        return service.retriveEntriesOfType(type, aliases).map((x) => x.data);
    };
    
    /**
    * @ngdoc method
    * @name umbraco.services.supportsCopy#retriveEntriesOfType
    * @methodOf umbraco.services.clipboardService
    *
    * @param type {string} umbraco A string defining the type of data to remove.
    * @param aliases {string} umbraco A array of strings providing the alias of the data you want to remove.
    * 
    * @description
    * Removes entries matching the given type and one of the provided aliases.
    */
    service.clearEntriesOfType = function(type, aliases) {
        
        var storage = retriveStorage();
        
        // Find entries that are NOT fulfilling the criteria for this nodeType and nodeTypesAliases.
        var filteretEntries = storage.entries.filter(
            (entry) => {
                return !(entry.type === type && aliases.filter(alias => alias === entry.alias).length > 0);
            }
        );
        
        storage.entries = filteretEntries;
        
        saveStorage(storage);
    };
    
    
    
    return service;
}
angular.module("umbraco.services").factory("clipboardService", clipboardService);
