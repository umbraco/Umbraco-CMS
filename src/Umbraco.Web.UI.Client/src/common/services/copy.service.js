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
function clipboardService(notificationsService, eventsService) {
    
    
    var STORAGE_KEY = "umbClipboardService";
    
    var supportsLocalStorage = function () {
        var test = "test";
        
        try {
            window.localStorage.setItem(test, test);
            window.localStorage.removeItem(test);
            return true;
        } catch (e) {
            return false;
        }
        
        return false;
    }
    
    var retriveStorage = function() {
        if (supportsLocalStorage === false) {
            return null;
        }
        
        
        var dataJSON;
        var dataString = window.localStorage.getItem(STORAGE_KEY);
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
        var storageString = JSON.stringify(storage)
        
        try {
            var storageJSON = JSON.parse(storageString);
            window.localStorage.setItem(STORAGE_KEY, storageString);
            
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
    service.isSupported = supportsLocalStorage;
    
    /**
    * @ngdoc method
    * @name umbraco.services.supportsCopy#copy
    * @methodOf umbraco.services.clipboardService
    *
    * @description
    * Determins wether the current browser is able to performe a copy-action.
    */
    service.hasEntriesOfType = function(type, aliases) {
        
        if(service.retriveEntriesOfType(type, aliases).length > 0) {
            return true;
        }
        
        return false;
    };
    
    service.retriveEntriesOfType = function(type, aliases) {
        
        var storage = retriveStorage();
        
        // Find entries that are furfilling the criterias for this nodeTYpe and nodeTypesAliases.
        var filteretEntries = storage.entries.filter(
            (entry) => {
                return (entry.type === type && aliases.filter(alias => alias === entry.alias).length > 0);
            }
        );
        
        return filteretEntries;
    };
    
    service.retriveDataOfType = function(type, aliases) {
        return service.retriveEntriesOfType(type, aliases).map((x) => x.data);
    };
    
    service.clearEntriesOfType = function(type, aliases) {
        
        var storage = retriveStorage();
        
        // Find entries that are NOT furfilling the criterias for this nodeTYpe and nodeTypesAliases.
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
