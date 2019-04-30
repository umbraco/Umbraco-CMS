function copyService(notificationsService, eventsService) {
    
    
    var STORAGE_KEY = "uCopyService";
    
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
            
            eventsService.emit("copyService.storageUpdate");
            
            return true;
        } catch(e) {
            return false;
        }
        
        return false;
    }
    
    
    var service = {};
    
    /**
    * @ngdoc method
    * @name umbraco.services.copyService#copy
    * @methodOf umbraco.services.copyService
    *
    * @description
    * Saves a JS-object to the LocalStage of copied entries.
    *
    */
    service.copy = function(nodeType, data) {
        
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
        
        var entry = {unique:key, nodeType:nodeType, data:shallowCloneData};
        storage.entries.push(entry);
        
        if (saveStorage(storage) === true) {
            notificationsService.success("Clipboard", "Copied to clipboard.");
        } else {
            notificationsService.success("Clipboard", "Couldnt copy this data to clipboard.");
        }
        
    };
    
    service.supportsCopy = supportsLocalStorage;
    
    service.hasEntriesOfType = function(nodeType, nodeTypeAliases) {
        
        if(service.retriveEntriesOfType(nodeType, nodeTypeAliases).length > 0) {
            return true;
        }
        
        return false;
    };
    
    service.retriveEntriesOfType = function(nodeType, nodeTypeAliases) {
        
        var storage = retriveStorage();
        
        // Find entries that are furfilling the criterias for this nodeTYpe and nodeTypesAliases.
        var filteretEntries = storage.entries.filter(
            (entry) => {
                return (entry.nodeType === nodeType && nodeTypeAliases.filter(alias => alias === entry.data.contentTypeAlias).length > 0);
            }
        );
        
        return filteretEntries;
    };
    
    service.retriveDataOfType = function(nodeType, nodeTypeAliases) {
        return service.retriveEntriesOfType(nodeType, nodeTypeAliases).map((x) => x.data);
    };
    
    service.clearEntriesOfType = function(nodeType, nodeTypeAliases) {
        
        var storage = retriveStorage();
        
        // Find entries that are NOT furfilling the criterias for this nodeTYpe and nodeTypesAliases.
        var filteretEntries = storage.entries.filter(
            (entry) => {
                return !(entry.nodeType === nodeType && nodeTypeAliases.filter(alias => alias === entry.data.contentTypeAlias).length > 0);
            }
        );
        
        storage.entries = filteretEntries;
        
        saveStorage(storage);
    };
    
    
    
    return service;
}
angular.module("umbraco.services").factory("copyService", copyService);
