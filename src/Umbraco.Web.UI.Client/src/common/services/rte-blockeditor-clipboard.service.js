/**
 * @ngdoc service
 * @name umbraco.services.rte-block-clipboard-service
 *
 * Handles clipboard resolvers for Block of RTE properties.
 *
 */
(function () {
    'use strict';



    /**
     * When performing a runtime copy of Block Editors entries, we copy the ElementType Data Model and inner IDs are kept identical, to ensure new IDs are changed on paste we need to provide a resolver for the ClipboardService.
     */
    angular.module('umbraco').run(['clipboardService', 'udiService', function (clipboardService, udiService) {

        function replaceUdi(obj, key, dataObject, markup) {
            var udi = obj[key];
            var newUdi = udiService.create("element");
            obj[key] = newUdi;
            dataObject.forEach((data) => {
                if (data.udi === udi) {
                    data.udi = newUdi;
                }
            });
            // make a attribute name of the key, by kebab casing it:
            var attrName = key.replace(/([a-z])([A-Z])/g, '$1-$2').toLowerCase();
            // replace the udi in the markup as well.
            var regex = new RegExp('data-'+attrName+'="'+udi+'"', "g");
            markup = markup.replace(regex, 'data-'+attrName+'="'+newUdi+'"');
            return markup;
        }
        function replaceUdisOfObject(obj, propValue, markup) {
            for (var k in obj) {
                if(k === "contentUdi") {
                  markup = replaceUdi(obj, k, propValue.contentData, markup);
                } else if(k === "settingsUdi") {
                  markup = replaceUdi(obj, k, propValue.settingsData, markup);
                } else {
                    // lets crawl through all properties of layout to make sure get captured all `contentUdi` and `settingsUdi` properties.
                    var propType = typeof obj[k];
                    if(propType != null && (propType === "object" || propType === "array")) {
                      markup = replaceUdisOfObject(obj[k], propValue, markup);
                    }
                }
            }
            return markup
        }


        function rawRteBlockResolver(propertyValue, propPasteResolverMethod) {
          if (propertyValue && typeof propertyValue === "object" && propertyValue.markup) {

              // object property of 'blocks' holds the data for the Block Editor.
              var value = propertyValue.blocks;

              // we got an object, and it has these three props then we are most likely dealing with a Block Editor.
              if ((value && value.layout !== undefined && value.contentData !== undefined && value.settingsData !== undefined)) {

                  // replaceUdisOfObject replaces udis of the value object(by instance reference), but also returns the updated markup (as we cant update the reference of a string).
                  propertyValue.markup = replaceUdisOfObject(value.layout, value, propertyValue.markup);

                  // run resolvers for inner properties of this Blocks content data.
                  if(value.contentData.length > 0) {
                      value.contentData.forEach((item) => {
                          for (var k in item) {
                              propPasteResolverMethod(item[k], clipboardService.TYPES.RAW);
                          }
                      });
                  }
                  // run resolvers for inner properties of this Blocks settings data.
                  if(value.settingsData.length > 0) {
                      value.settingsData.forEach((item) => {
                          for (var k in item) {
                              propPasteResolverMethod(item[k], clipboardService.TYPES.RAW);
                          }
                      });
                  }

              }
          }
      }

        function elementTypeBlockResolver(obj, propPasteResolverMethod) {
            // we could filter for specific Property Editor Aliases, but as the Block Editor structure can be used by many Property Editor we do not in this code know a good way to detect that this is a Block Editor and will therefor leave it to the value structure to determin this.
            rawRteBlockResolver(obj.value, propPasteResolverMethod);
        }

        clipboardService.registerPastePropertyResolver(elementTypeBlockResolver, clipboardService.TYPES.ELEMENT_TYPE);
        clipboardService.registerPastePropertyResolver(rawRteBlockResolver, clipboardService.TYPES.RAW);

    }]);

})();
