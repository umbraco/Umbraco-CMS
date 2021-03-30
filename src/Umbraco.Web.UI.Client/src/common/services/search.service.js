/**
 * @ngdoc service
 * @name umbraco.services.searchService
 *
 *
 * @description
 * Service for handling the main application search, can currently search content, media and members
 *
 * ##usage
 * To use, simply inject the searchService into any controller that needs it, and make
 * sure the umbraco.services module is accesible - which it should be by default.
 *
 * <pre>
 *      searchService.searchMembers({term: 'bob'}).then(function(results){
 *          results.forEach(function(result){
 *                  //returns:
 *                  {name: "name", id: 1234, menuUrl: "url", editorPath: "url", metaData: {}, subtitle: "/path/etc" }
 *           })
 *           var result =
 *       })
 * </pre>
 */
angular.module('umbraco.services')
    .factory('searchService', function (entityResource, $injector, searchResultFormatter) {
        
        return {

            /**
            * @ngdoc method
            * @name umbraco.services.searchService#searchMembers
            * @methodOf umbraco.services.searchService
            *
            * @description
            * Searches the default member search index
            * @param {Object} args argument object
            * @param {String} args.term seach term
            * @returns {Promise} returns promise containing all matching members
            */
            searchMembers: function (args) {

                if (!args.term) {
                    throw "args.term is required";
                }

                return entityResource.search(args.term, "Member", args.searchFrom)
                    .then(data => {
                        data.forEach(item => searchResultFormatter.configureMemberResult(item));
                        return data;
                    });
            },

            /**
            * @ngdoc method
            * @name umbraco.services.searchService#searchContent
            * @methodOf umbraco.services.searchService
            *
            * @description
            * Searches the default internal content search index
            * @param {Object} args argument object
            * @param {String} args.term seach term
            * @returns {Promise} returns promise containing all matching content items
            */
            searchContent: function (args) {

                if (!args.term) {
                    throw "args.term is required";
                }

                return entityResource.search(args.term, "Document", args.searchFrom, args.canceler, args.dataTypeKey)
                    .then(data => {
                        data.forEach(item => searchResultFormatter.configureContentResult(item));
                        return data;
                    });
                
            },

            /**
            * @ngdoc method
            * @name umbraco.services.searchService#searchMedia
            * @methodOf umbraco.services.searchService
            *
            * @description
            * Searches the default media search index
            * @param {Object} args argument object
            * @param {String} args.term seach term
            * @returns {Promise} returns promise containing all matching media items
            */
            searchMedia: function (args) {

                if (!args.term) {
                    throw "args.term is required";
                }

                return entityResource.search(args.term, "Media", args.searchFrom, args.canceler, args.dataTypeKey)
                    .then(data => {
                        data.forEach(item => searchResultFormatter.configureMediaResult(item));
                        return data;
                    });
            },

            /**
            * @ngdoc method
            * @name umbraco.services.searchService#searchAll
            * @methodOf umbraco.services.searchService
            *
            * @description
            * Searches all available indexes and returns all results in one collection
            * @param {Object} args argument object
            * @param {String} args.term seach term
            * @returns {Promise} returns promise containing all matching items
            */
            searchAll: function (args) {

                if (!args.term) {
                    throw "args.term is required";
                }

                return entityResource.searchAll(args.term, args.canceler).then(data => {
                    Object.values(data).forEach(resultByType => {
                        //we need to format the search result data to include things like the subtitle, urls, etc...
                        // this is done with registered angular services as part of the SearchableTreeAttribute, if that
                        // is not found, than we format with the default formatter
                        var formatterMethod = searchResultFormatter.configureDefaultResult;
                        //check if a custom formatter is specified...
                        if (resultByType.jsSvc) {
                            var searchFormatterService = $injector.get(resultByType.jsSvc);
                            if (searchFormatterService) {
                                if (!resultByType.jsMethod) {
                                    resultByType.jsMethod = "format";
                                }
                                formatterMethod = searchFormatterService[resultByType.jsMethod];

                                if (!formatterMethod) {
                                    throw "The method " + resultByType.jsMethod + " on the angular service " + resultByType.jsSvc + " could not be found";
                                }
                            }
                        }
                        //now apply the formatter for each result
                        resultByType.results.forEach(item => {
                            formatterMethod.apply(this, [item, resultByType.treeAlias, resultByType.appAlias]);
                        });

                    });

                    return data;
                });
            },

            // TODO: This doesn't do anything!
            setCurrent: function (sectionAlias) {
                var currentSection = sectionAlias;
            }
        };
    });
