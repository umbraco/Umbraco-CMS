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
        var dataString = window.localStorage.getItem(STORAGE_KEY);
        if (dataString == null) {
            return null;
        }
        return JSON.parse(dataString);
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
    * Saves a JS-object to the LocalStage of copied items.
    *
    */
    service.copy = function(nodeType, data) {
        
        var storage = retriveStorage();
        if(storage === null) {
            storage = new Object();
        }
        
        if(storage.items === undefined) {
            storage.items = [];
        }
        
        var shallowCloneData = Object.assign({}, data);// Notice only a shallow copy, since we dont need to deep copy. (that will happen when storing the data)
        
        delete shallowCloneData.key;
        delete shallowCloneData.$$hashKey;
        
        var entry = {nodeType:nodeType, data:shallowCloneData};
        storage.items.push(entry);
        
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
        
        if (storage === null) {
            return [];
        }
        
        var filteretEntries = storage.items.filter(
            (item) => item.nodeType === nodeType
        );
        if (nodeTypeAliases) {
            filteretEntries = filteretEntries.filter(
                (item) => {
                    return nodeTypeAliases.filter(alias => alias === item.data.contentTypeAlias).length > 0;
                }
            );
        }
        return filteretEntries;
    };
    
    
    service.retriveDataOfType = function(nodeType, nodeTypeAliases) {
        return service.retriveEntriesOfType(nodeType, nodeTypeAliases).map((x) => x.data);
    };
    
    
    
    return service;
}
angular.module("umbraco.services").factory("copyService", copyService);
