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
 *          angular.forEach(results, function(result){
 *                  //returns:
 *                  {name: "name", id: 1234, menuUrl: "url", editorPath: "url", metaData: {}, subtitle: "/path/etc" }
 *           })          
 *           var result = 
 *       }) 
 * </pre> 
 */
angular.module('umbraco.services')
.factory('searchService', function ($q, $log, entityResource, contentResource, umbRequestHelper) {

    function configureMemberResult(member) {
        member.menuUrl = umbRequestHelper.getApiUrl("memberTreeBaseUrl", "GetMenu", [{ id: member.id }, { application: 'member' }]);
        member.editorPath = "member/member/edit/" + (member.key ? member.key : member.id);
        angular.extend(member.metaData, { treeAlias: "member" });
        member.subTitle = member.metaData.Email;
    }
    
    function configureMediaResult(media)
    {
        media.menuUrl = umbRequestHelper.getApiUrl("mediaTreeBaseUrl", "GetMenu", [{ id: media.id }, { application: 'media' }]);
        media.editorPath = "media/media/edit/" + media.id;
        angular.extend(media.metaData, { treeAlias: "media" });
    }
    
    function configureContentResult(content) {
        content.menuUrl = umbRequestHelper.getApiUrl("contentTreeBaseUrl", "GetMenu", [{ id: content.id }, { application: 'content' }]);
        content.editorPath = "content/content/edit/" + content.id;
        angular.extend(content.metaData, { treeAlias: "content" });
        content.subTitle = content.metaData.Url;        
    }

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
        searchMembers: function(args) {

            if (!args.term) {
                throw "args.term is required";
            }

            return entityResource.search(args.term, "Member", args.searchFrom).then(function (data) {
                _.each(data, function(item) {
                    configureMemberResult(item);
                });         
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
        searchContent: function(args) {

            if (!args.term) {
                throw "args.term is required";
            }

            return entityResource.search(args.term, "Document", args.searchFrom, args.canceler).then(function (data) {
                _.each(data, function (item) {
                    configureContentResult(item);
                });
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
        searchMedia: function(args) {

            if (!args.term) {
                throw "args.term is required";
            }

            return entityResource.search(args.term, "Media", args.searchFrom).then(function (data) {
                _.each(data, function (item) {
                    configureMediaResult(item);
                });
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

            return entityResource.searchAll(args.term, args.canceler).then(function (data) {

                _.each(data, function(resultByType) {
                    switch(resultByType.type) {
                        case "Document":
                            _.each(resultByType.results, function (item) {
                                configureContentResult(item);
                            });
                            break;
                        case "Media":
                            _.each(resultByType.results, function (item) {
                                configureMediaResult(item);
                            });                            
                            break;
                        case "Member":
                            _.each(resultByType.results, function (item) {
                                configureMemberResult(item);
                            });                            
                            break;
                    }
                });

                return data;
            });
            
        },

        //TODO: This doesn't do anything!
        setCurrent: function(sectionAlias) {

            var currentSection = sectionAlias;
        }
    };
});