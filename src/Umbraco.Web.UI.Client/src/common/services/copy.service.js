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
        
        
        if(nodeType === "content" || nodeType === "element") {
            if(data.contentTypeAlias === undefined) {
                // missing contentTypeAlias... then we cant copy jet.
                success = false;
            }
        }
        
        /**
        Will need to store things differently...
        
        Can we copy real IpublishedContent...
        
        And should I wrap data into a entry-object knowing about the data type..
        
        */
        
        delete data.key;
        delete data.$$hashKey;
        
        if(storage.items === undefined) {
            storage.items = [];
        }
        
        storage.items.push({nodeType:nodeType, data:data});
        
        if (saveStorage(storage) === true) {
            notificationsService.success("", "Copied to clipboard.");
        } else {
            notificationsService.success("", "Couldnt copy this data to clipboard.");
        }
        
    };
    
    service.supportsCopy = supportsLocalStorage;
    
    service.hasDataOfType = function(nodeType, nodeTypeAliases) {
        
        if(service.retriveDataOfType(nodeType, nodeTypeAliases).length > 0) {
            return true;
        }
        
        return false;
    };
    
    service.retriveDataOfType = function(nodeType, nodeTypeAliases) {
        
        var storage = retriveStorage();
        
        if (storage === null) {
            return [];
        }
        
        var itemsOfType = storage.items.filter(
            (item) => item.nodeType === nodeType
        );
        if (nodeTypeAliases) {
            return itemsOfType.filter(
                (item) => {
                    return nodeTypeAliases.filter(alias => alias === item.data.contentTypeAlias).length > 0;
                }
            );
        } else {
            return itemsOfType;
        }
        
    };
    
    
    
    return service;
}
angular.module("umbraco.services").factory("copyService", copyService);
