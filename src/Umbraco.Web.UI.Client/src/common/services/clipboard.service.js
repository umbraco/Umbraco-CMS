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
function clipboardService(notificationsService, eventsService, localStorageService, iconHelper) {


    const TYPES = {};
    TYPES.ELEMENT_TYPE = "elementType";
    TYPES.RAW = "raw";

    var clearPropertyResolvers = {};
    var pastePropertyResolvers = {};
    var clipboardTypeResolvers = {};

    clipboardTypeResolvers[TYPES.ELEMENT_TYPE] = function(data, propMethod) {
        for (var t = 0; t < data.variants[0].tabs.length; t++) {
            var tab = data.variants[0].tabs[t];
            for (var p = 0; p < tab.properties.length; p++) {
                var prop = tab.properties[p];
                propMethod(prop, TYPES.ELEMENT_TYPE);
            }
        }
    }
    clipboardTypeResolvers[TYPES.RAW] = function(data, propMethod) {
        for (var p = 0; p < data.length; p++) {
            propMethod(data[p], TYPES.RAW);
        }
    }


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


    function resolvePropertyForStorage(prop, type) {

        type = type || "raw";
        var resolvers = clearPropertyResolvers[type];

        for (var i=0; i<resolvers.length; i++) {
            resolvers[i](prop, resolvePropertyForStorage);
        }

    }

    var prepareEntryForStorage = function(type, entryData, firstLevelClearupMethod) {

        var cloneData = Utilities.copy(entryData);
        if (firstLevelClearupMethod != undefined) {
            firstLevelClearupMethod(cloneData);
        }

        var typeResolver = clipboardTypeResolvers[type];
        if(typeResolver) {
            typeResolver(cloneData, resolvePropertyForStorage);
        } else {
            console.warn("Umbraco.service.clipboardService has no type resolver for '" + type + "'.");
        }

        return cloneData;
    }

    var isEntryCompatible = function(entry, type, allowedAliases) {
        return entry.type === type
        &&
        (
            (entry.alias && allowedAliases.filter(allowedAlias => allowedAlias === entry.alias).length > 0)
            ||
            (entry.aliases && entry.aliases.filter(entryAlias => allowedAliases.filter(allowedAlias => allowedAlias === entryAlias).length > 0).length === entry.aliases.length)
        );
    }




    function resolvePropertyForPaste(prop, type) {

        type = type || "raw";
        var resolvers = pastePropertyResolvers[type];

        for (var i=0; i<resolvers.length; i++) {
            resolvers[i](prop, resolvePropertyForPaste);
        }
    }



    var service = {};

    /**
     * Default types to store in clipboard.
     */
    service.TYPES = TYPES;


    /**
    * @ngdoc method
    * @name umbraco.services.clipboardService#parseContentForPaste
    * @methodOf umbraco.services.clipboardService
    *
    * @param {object} function The content entry to be cloned and resolved.
    *
    * @description
    * Executed registered property resolvers for inner properties, to be done on pasting a clipbaord content entry.
    *
    */
    service.parseContentForPaste = function(pasteEntryData, type) {
        var cloneData = Utilities.copy(pasteEntryData);

        var typeResolver = clipboardTypeResolvers[type];
        if(typeResolver) {
            typeResolver(cloneData, resolvePropertyForPaste);
        } else {
            console.warn("Umbraco.service.clipboardService has no type resolver for '" + type + "'.");
        }

        return cloneData;
    }


    /**
    * @ngdoc method
    * @name umbraco.services.clipboardService#registrerClearPropertyResolver
    * @methodOf umbraco.services.clipboardService
    *
    * @param {string} function A method executed for every property and inner properties copied.
    * @param {string} string A string representing the property type format for this resolver to execute for.
    *
    * @description
    * Executed for all properties of given type when performing a copy action.
    *
    * @deprecated Incorrect spelling please use 'registerClearPropertyResolver'
    */
    service.registrerClearPropertyResolver = function(resolver, type) {
        this.registerClearPropertyResolver(resolver, type);
    };

    /**
    * @ngdoc method
    * @name umbraco.services.clipboardService#registerClearPropertyResolver
    * @methodOf umbraco.services.clipboardService
    *
    * @param {method} function A method executed for every property and inner properties copied. Notice the method both needs to deal with retriving a property object {alias:..editor:..value:... ,...} and the raw property value as if the property is an inner property of a nested property.
    * @param {string} string A string representing the property type format for this resolver to execute for.
    *
    * @description
    * Executed for all properties of given type when performing a copy action.
    */
    service.registerClearPropertyResolver = function(resolver, type) {
        type = type || "raw";
        if(!clearPropertyResolvers[type]) {
            clearPropertyResolvers[type] = [];
        }
        clearPropertyResolvers[type].push(resolver);
    };

    /**
    * @ngdoc method
    * @name umbraco.services.clipboardService#registerPastePropertyResolver
    * @methodOf umbraco.services.clipboardService
    *
    * @param {method} function A method executed for every property and inner properties copied. Notice the method both needs to deal with retriving a property object {alias:..editor:..value:... ,...} and the raw property value as if the property is an inner property of a nested property.
    * @param {string} string A string representing the property type format for this resolver to execute for.
    *
    * @description
    * Executed for all properties of given type when performing a paste action.
    */
    service.registerPastePropertyResolver = function(resolver, type) {
        type = type || "raw";
        if(!pastePropertyResolvers[type]) {
            pastePropertyResolvers[type] = [];
        }
        pastePropertyResolvers[type].push(resolver);
    };


    /**
    * @ngdoc method
    * @name umbraco.services.clipboardService#registrerTypeResolvers
    * @methodOf umbraco.services.clipboardService
    *
    * @param {method} function A method for to execute resolvers for each properties of the specific type.
    * @param {string} string A string representing the content type format for this resolver to execute for.
    *
    * @description
    * Executed for all properties including inner properties when performing a paste action.
    */
    service.registrerTypeResolvers = function(resolver, type) {
        if(!clipboardTypeResolvers[type]) {
            clipboardTypeResolvers[type] = [];
        }
        clipboardTypeResolvers[type].push(resolver);
    };


    /**
    * @ngdoc method
    * @name umbraco.services.clipboardService#copy
    * @methodOf umbraco.services.clipboardService
    *
    * @param {string} type A string defining the type of data to storing, example: 'elementType', 'contentNode'
    * @param {string} alias A string defining the alias of the data to store, example: 'product'
    * @param {object} entry A object containing the properties to be saved, this could be the object of a ElementType, ContentNode, ...
    * @param {string} displayLabel (optional) A string swetting the label to display when showing paste entries.
    * @param {string} displayIcon (optional) A string setting the icon to display when showing paste entries.
    * @param {string} uniqueKey (optional) A string prodiving an identifier for this entry, existing entries with this key will be removed to ensure that you only have the latest copy of this data.
    *
    * @description
    * Saves a single JS-object with a type and alias to the clipboard.
    */
    service.copy = function(type, alias, data, displayLabel, displayIcon, uniqueKey, firstLevelClearupMethod) {

        var storage = retriveStorage();

        displayLabel = displayLabel || data.name;
        displayIcon = displayIcon || iconHelper.convertFromLegacyIcon(data.icon);
        uniqueKey = uniqueKey || data.key || console.error("missing unique key for this content");

        // remove previous copies of this entry:
        storage.entries = storage.entries.filter(
            (entry) => {
                return entry.unique !== uniqueKey;
            }
        );

        var entry = {unique:uniqueKey, type:type, alias:alias, data:prepareEntryForStorage(type, data, firstLevelClearupMethod), label:displayLabel, icon:displayIcon, date:Date.now()};
        storage.entries.push(entry);

        if (saveStorage(storage) === true) {
            notificationsService.success("Clipboard", "Copied to clipboard.");
        } else {
            notificationsService.error("Clipboard", "Couldnt copy this data to clipboard.");
        }

    };


    /**
    * @ngdoc method
    * @name umbraco.services.clipboardService#copyArray
    * @methodOf umbraco.services.clipboardService
    *
    * @param {string} type A string defining the type of data to storing, example: 'elementTypeArray', 'contentNodeArray'
    * @param {string} aliases An array of strings defining the alias of the data to store, example: ['banana', 'apple']
    * @param {object} datas An array of objects containing the properties to be saved, example: [ElementType, ElementType, ...]
    * @param {string} displayLabel A string setting the label to display when showing paste entries.
    * @param {string} displayIcon A string setting the icon to display when showing paste entries.
    * @param {string} uniqueKey A string prodiving an identifier for this entry, existing entries with this key will be removed to ensure that you only have the latest copy of this data.
    * @param {string} firstLevelClearupMethod A string prodiving an identifier for this entry, existing entries with this key will be removed to ensure that you only have the latest copy of this data.
    *
    * @description
    * Saves a single JS-object with a type and alias to the clipboard.
    */
    service.copyArray = function(type, aliases, datas, displayLabel, displayIcon, uniqueKey, firstLevelClearupMethod) {

        if (type === "elementTypeArray") {
            type = "elementType";
        }

        var storage = retriveStorage();

        // Clean up each entry
        var copiedDatas = datas.map(data => prepareEntryForStorage(type, data, firstLevelClearupMethod));

        // remove previous copies of this entry:
        storage.entries = storage.entries.filter(
            (entry) => {
                return entry.unique !== uniqueKey;
            }
        );

        var entry = {unique:uniqueKey, type:type, aliases:aliases, data:copiedDatas, label:displayLabel, icon:displayIcon, date:Date.now()};
        storage.entries.push(entry);

        if (saveStorage(storage) === true) {
            notificationsService.success("Clipboard", "Copied to clipboard.");
        } else {
            notificationsService.error("Clipboard", "Couldnt copy this data to clipboard.");
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
    * @param {string} type A string defining the type of data test for.
    * @param {string} aliases A array of strings providing the alias of the data you want to test for.
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
    * @param {string} type A string defining the type of data to recive.
    * @param {string} aliases A array of strings providing the alias of the data you want to recive.
    *
    * @description
    * Returns an array of entries matching the given type and one of the provided aliases.
    */
    service.retriveEntriesOfType = function(type, allowedAliases) {

        var storage = retriveStorage();

        // Find entries that are fulfilling the criteria for this nodeType and nodeTypesAliases.
        var filteretEntries = storage.entries.filter(
            (entry) => {
                return isEntryCompatible(entry, type, allowedAliases);
            }
        );

        return filteretEntries;
    };

    /**
    * @ngdoc method
    * @name umbraco.services.supportsCopy#retriveEntriesOfType
    * @methodOf umbraco.services.clipboardService
    *
    * @param {string} type A string defining the type of data to recive.
    * @param {string} aliases A array of strings providing the alias of the data you want to recive.
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
    * @param {string} type A string defining the type of data to remove.
    * @param {string} aliases A array of strings providing the alias of the data you want to remove.
    *
    * @description
    * Removes entries matching the given type and one of the provided aliases.
    */
    service.clearEntriesOfType = function(type, allowedAliases) {

        var storage = retriveStorage();

        // Find entries that are NOT fulfilling the criteria for this nodeType and nodeTypesAliases.
        var filteretEntries = storage.entries.filter(
            (entry) => {
                return !isEntryCompatible(entry, type, allowedAliases);
            }
        );

        storage.entries = filteretEntries;

        saveStorage(storage);
    };



    return service;
}


angular.module("umbraco.services").factory("clipboardService", clipboardService);

