angular.module('umbraco.services')
.factory('searchService', function ($q, $log, entityResource, contentResource, umbRequestHelper) {

    function configureMemberResult(member) {
        member.menuUrl = umbRequestHelper.getApiUrl("memberTreeBaseUrl", "GetMenu", [{ id: member.id }, { application: 'member' }]);
        member.editorPath = "member/member/edit/" + (member.key ? member.key : member.id);
        member.metaData = { treeAlias: "member" };
        member.subTitle = member.metaData.Email;
    }
    
    function configureMediaResult(media)
    {
        media.menuUrl = umbRequestHelper.getApiUrl("mediaTreeBaseUrl", "GetMenu", [{ id: media.id }, { application: 'media' }]);
        media.editorPath = "media/media/edit/" + media.id;
        media.metaData = { treeAlias: "media" };
    }
    
    function configureContentResult(content) {
        content.menuUrl = umbRequestHelper.getApiUrl("contentTreeBaseUrl", "GetMenu", [{ id: content.id }, { application: 'content' }]);
        content.editorPath = "content/content/edit/" + content.id;
        content.metaData = { treeAlias: "content" };
        content.subTitle = content.metaData.Url;        
    }

    return {
        searchMembers: function(args) {

            if (!args.term) {
                throw "args.term is required";
            }

            return entityResource.search(args.term, "Member").then(function (data) {
                _.each(data, function(item) {
                    configureMemberResult(item);
                });         
                return data;
            });
        },
        searchContent: function(args) {

            if (!args.term) {
                throw "args.term is required";
            }

            return entityResource.search(args.term, "Document").then(function (data) {
                _.each(data, function (item) {
                    configureContentResult(item);
                });
                return data;
            });
        },
        searchMedia: function(args) {

            if (!args.term) {
                throw "args.term is required";
            }

            return entityResource.search(args.term, "Media").then(function (data) {
                _.each(data, function (item) {
                    configureMediaResult(item);
                });
                return data;
            });
        },
        searchAll: function (args) {
            
            if (!args.term) {
                throw "args.term is required";
            }

            return entityResource.searchAll(args.term).then(function (data) {

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

        setCurrent: function(sectionAlias) {
            currentSection = sectionAlias;
        }
    };
});