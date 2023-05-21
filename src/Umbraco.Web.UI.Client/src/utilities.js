/**
 * A friendly utility collection to replace AngularJs' ng-functions
 * If it doesn't exist here, it's probably available as vanilla JS
 * 
 * Still carries a dependency on underscore, but if usages of underscore from 
 * elsewhere in the codebase can instead use these methods, the underscore
 * dependency will be nicely abstracted and can be removed/swapped later
 * 
 * This collection is open to extension...
 */
(function (window) {

    /**
     * Equivalent to angular.noop
     */
    const noop = () => { };

    /**
     * Facade to angular.copy
     */
    const copy = (src, dst) => angular.copy(src, dst);

    /**
     * Equivalent to angular.isArray
     */
    const isArray = val => Array.isArray(val) || val instanceof Array;

    /**
     * Facade to angular.equals
     */
    const equals = (a, b) => angular.equals(a, b);
    
    /**
     * Facade to angular.extend
     * Use this with Angular objects, for vanilla JS objects, use Object.assign()
     * This is an alias as it to allow passing an unknown number of arguments
     */
    const extend = angular.extend;

    /**
     * Equivalent to angular.isFunction
     */
    const isFunction = val => typeof val === 'function';

    /**
     * Equivalent to angular.isUndefined
     */
    const isUndefined = val => typeof val === 'undefined';

    /**
     * Equivalent to angular.isDefined. Inverts result of const isUndefined
     */
    const isDefined = val => !isUndefined(val);

    /**
     * Equivalent to angular.isString
     */
    const isString = val => typeof val === 'string';

    /**
     * Equivalent to angular.isNumber
     */
    const isNumber = val => typeof val === 'number';

    /**
     * Equivalent to angular.isObject
     */
    const isObject = val => val !== null && typeof val === 'object';

    const isWindow = obj => obj && obj.window === obj;

    const isScope = obj => obj && obj.$evalAsync && obj.$watch;

    const toJsonReplacer = (key, value) => {
        var val = value;
        if (typeof key === 'string' && key.charAt(0) === '$' && key.charAt(1) === '$') {
            val = undefined;
        } else if (isWindow(value)) {
            val = '$WINDOW';
        } else if (value && window.document === value) {
            val = '$DOCUMENT';
        } else if (isScope(value)) {
            val = '$SCOPE';
        }
        return val;
    };

    /**
     * Equivalent to angular.toJson
     */
    const toJson = (obj, pretty) => {
        if (isUndefined(obj)) return undefined;
        if (!isNumber(pretty)) {
            pretty = pretty ? 2 : null;
        }
        return JSON.stringify(obj, toJsonReplacer, pretty);
    };

    /**
     * Equivalent to angular.fromJson
     */
    const fromJson = (val) => {
        if (!isString(val)) {
            return val;
        }
        return JSON.parse(val);
    };

    /**
     * Not equivalent to angular.forEach. But like the angularJS method this does not fail on null or undefined.
     */
    const forEach = (obj, iterator) => {
        if (obj && isArray(obj)) {
            return obj.forEach(iterator);
        }
        return obj;
    };

    const MediaUploader = function (Upload, mediaHelper, mediaTypeHelper, localizationService, overlayService, mediaTypeResource) {
        const umbracoSettings = Umbraco.Sys.ServerVariables.umbracoSettings;
        const allowedUploadFiles = mediaHelper.formatFileTypes(umbracoSettings.allowedUploadFiles);
        const allowedImageFileTypes = mediaHelper.formatFileTypes(umbracoSettings.imageFileTypes);
        const allowedFileTypes = `${allowedUploadFiles},${allowedImageFileTypes}`;
        const disallowedFileTypes = mediaHelper.formatFileTypes(umbracoSettings.disallowedUploadFiles);
        const maxFileSize = umbracoSettings.maxFileSize !== '' ? `${umbracoSettings.maxFileSize} KB` : '';
        
        const events = {};
        const translations = {};

        let initialized = false;
        let uploadURL = '';
        let allowedMediaTypes = [];
        let queue = [];
        let invalidEntries = [];

        function init (options) {
            uploadURL = options.uploadURL;

            return new Promise((resolve, reject) => {
                const promises = [
                    mediaTypeResource.getAllFiltered(options.allowedMediaTypeAliases),
                    localizationService.localizeMany(["media_disallowedFileType", "media_maxFileSize", "defaultdialogs_selectMediaType"])
                ];

                Promise.all(promises).then(values => {
                    const mediaTypes = values[0];
                    const translationValues = values[1];

                    allowedMediaTypes = mediaTypes;
                    translations.disallowedFileType = translationValues[0];
                    translations.maxFileSize = translationValues[1] + " " + maxFileSize;
                    translations.selectMediaTypeDialogTitle = translationValues[2];
                    initialized = true;
                    resolve();
                }, (reason) => {
                    reject(reason);
                });
            });
        }

        function requestUpload (files) {
            if (!initialized) {
                throw 'MediaUploader is not initialized';
            }

            const validBatch = [];
            const uploadItems = createUploadItemsFromFiles(files);

            // Validate based on server allowed file types
            uploadItems.forEach(item => {
                const isAllowedFileType = Upload.validatePattern(item.file, allowedFileTypes);
                const isDisallowedFileType = Upload.validatePattern(item.file, disallowedFileTypes);
                const underMaxFileSize = maxFileSize ? validateMaxFileSize(item.file, maxFileSize) : true;

                if (isAllowedFileType && !isDisallowedFileType && underMaxFileSize) {
                    _acceptMediaEntry(item.mediaEntry);
                    validBatch.push(item);
                    return;
                }

                if (!isAllowedFileType || isDisallowedFileType) {
                    _rejectMediaEntry(item.mediaEntry, { type: 'pattern', message: translations.disallowedFileType });
                    return;
                }

                if (!underMaxFileSize) {
                    _rejectMediaEntry(item.mediaEntry, { type: 'maxSize', message: translations.maxFileSize });
                    return;
                }
            });

            _addItemsToQueue(validBatch);
            _emit('queueStarted');
            _processQueue();
        }

        function _acceptMediaEntry (mediaEntry) {
            _emit('mediaEntryAccepted', { mediaEntry });
        }

        function _rejectMediaEntry (mediaEntry, reason) {
            mediaEntry.error = true;
            mediaEntry.errorType = {};
            mediaEntry.errorType[reason.type] = true;
            mediaEntry.errorText = reason.message;
            
            invalidEntries.push(mediaEntry);
            _emit('mediaEntryRejected', { mediaEntry });
        }
 
        function createUploadItemsFromFiles (files) {
            // angular complains about "Illegal invocation" if the file is part of the model.value
            // so we have to keep them separate
            return files.map(file => {
                const mediaEntry = {
                    key: String.CreateGuid(),
                    name: file.name,
                    $uploadProgress: 0,
                    $dataURL: ''
                };

                if (file.type.includes('image')) {
                    Upload.base64DataUrl(file).then(function(url) {    
                        mediaEntry.$dataURL = url;
                    });
                } else {
                    mediaEntry.$extension = mediaHelper.getFileExtension(file.name);
                }

                return {
                    mediaEntry,
                    file
                };
            });
        };

        function validateMaxFileSize (file, val) {
            return file.size - 0.1 <= Upload.translateScalars(val);
        }
    
        function _upload(queueItem) {
            const mediaEntry = queueItem.mediaEntry;

            _emit('uploadStarted', { mediaEntry });
            Upload.upload({
                    url: uploadURL,
                    file: queueItem.file
                })
                .progress(function(evt) {
                    var progressPercentage = parseInt(100.0 * evt.loaded / evt.total, 10);
                    mediaEntry.$uploadProgress = progressPercentage;
                })
                .success(function (data) {
                    _emit('uploadSuccess', { mediaEntry, ...data });
                    _processQueue();
                })
                .error(function(error) {
                    _emit('uploadError', { mediaEntry });
                    _rejectMediaEntry(mediaEntry, { type: 'server', message: error.Message });
                    _processQueue();
                });
        }

        function _addItemsToQueue (queueItems) {
            queue = [...queue, ...queueItems];
        }

        function _processQueue () {
            const nextItem = queue.shift();

            // queue is empty
            if (!nextItem) {
                _emit('queueCompleted');
                return;
            }

            _getMatchedMediaType(nextItem.file).then(mediaType => {
                nextItem.mediaEntry.mediaTypeAlias = mediaType.alias;
                nextItem.mediaEntry.$icon = mediaType.icon;
                _upload(nextItem);
            }, () => {
                _rejectMediaEntry(nextItem.mediaEntry, { type: 'pattern', message: translations.disallowedFileType });
                _processQueue();
            });
        }

        function _getMatchedMediaType(file) {
            return new Promise((resolve, reject) => {
                const uploadFileExtension = mediaHelper.getFileExtension(file.name);
                const matchedMediaTypes = mediaTypeHelper.getTypeAcceptingFileExtensions(allowedMediaTypes, [uploadFileExtension]);
                
                if (matchedMediaTypes.length === 0) {
                    reject();
                    return;
                };
                
                if (matchedMediaTypes.length === 1) {
                    resolve(matchedMediaTypes[0]);
                    return;
                };

                // when we get all media types, the "File" media type will always show up because it accepts all file extensions.
                // If we don't remove it from the list we will always show the picker.
                const matchedMediaTypesNoFile = matchedMediaTypes.filter(mediaType => mediaType.alias !== "File");
                if (matchedMediaTypesNoFile.length === 1) {
                    resolve(matchedMediaTypesNoFile[0]);
                    return;
                };
    
                if (matchedMediaTypes.length > 1) {
                    _chooseMediaTypeDialog(matchedMediaTypes, file)
                    .then((selectedMediaType) => {
                        resolve(selectedMediaType);
                    }, () => {
                        reject();
                    });
                };
            });
        }

        function _chooseMediaTypeDialog(mediaTypes, file) {
            return new Promise((resolve, reject) => {
                const dialog = {
                    view: "itempicker",
                    filter: mediaTypes.length > 8,
                    availableItems: mediaTypes,
                    submit: function (model) {
                        resolve(model.selectedItem);
                        overlayService.close();
                    },
                    close: function () {
                        reject();
                        overlayService.close();
                    }
                };

                dialog.title = translations.selectMediaTypeDialogTitle;
                dialog.subtitle = file.name;
                overlayService.open(dialog);
            });
        }

        function _emit(name, data) {
            if (!events[name]) return;
            events[name].forEach(callback => callback({name}, data));
        }

        function on(name, callback) {
            if (typeof callback !== 'function') return;

            const unsubscribe = function () {
                events[name] = events[name].filter(cb => cb !== callback);
            }

            events[name] = events[name] || [];
            events[name].push(callback);
            return unsubscribe;
        }
    
        return {
            init,
            requestUpload,
            on
        }
    }

    let _utilities = {
        noop: noop,
        copy: copy,
        isArray: isArray,
        equals: equals,
        extend: extend,
        isFunction: isFunction,
        isUndefined: isUndefined,
        isDefined: isDefined,
        isString: isString,
        isNumber: isNumber,
        isObject: isObject,
        fromJson: fromJson,
        toJson: toJson,
        forEach: forEach,
        MediaUploader: MediaUploader
    };

    if (typeof (window.Utilities) === 'undefined') {
        window.Utilities = _utilities;
    }
})(window);
