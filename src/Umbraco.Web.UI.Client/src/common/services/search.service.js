angular.module('umbraco.services')
.factory('searchService', function ($q, $log, entityResource, contentResource, umbRequestHelper) {

    function configureMemberResult(el) {
        el.menuUrl = umbRequestHelper.getApiUrl("memberTreeBaseUrl", "GetMenu", [{ id: el.Id }, { application: 'member' }]);
        el.metaData = { treeAlias: "member" };
        el.title = el.Fields.nodeName;
        el.subTitle = el.Fields.email;
        el.id = el.Id;
    }
    
    function configureMediaResult(el)
    {
        el.menuUrl = umbRequestHelper.getApiUrl("mediaTreeBaseUrl", "GetMenu", [{ id: el.Id }, { application: 'media' }]);
        el.metaData = { treeAlias: "media" };
        el.title = el.Fields.nodeName;
        el.id = el.Id;
    }
    
    function configureContentResult(el) {
        el.menuUrl = umbRequestHelper.getApiUrl("contentTreeBaseUrl", "GetMenu", [{ id: el.Id }, { application: 'content' }]);
        el.metaData = { treeAlias: "content" };
        el.title = el.Fields.nodeName;
        el.id = el.Id;

        contentResource.getNiceUrl(el.Id).then(function (url) {
            el.subTitle = angular.fromJson(url);
        });
    }

    return {
        searchMembers: function(args) {

            if (!args.term) {
                throw "args.term is required";
            }

            return entityResource.search(args.term, "Member").then(function (data) {

                _.each(data, function(el) {
                    configureMemberResult(el);
                });

                var results = (args.results && angular.isArray(args.results)) ? args.results : [];
                
                results.push({
                    icon: "icon-user",
                    editor: "member/member/edit/",
                    matches: data
                });

                return results;
            });
        },
        searchContent: function(args) {

            if (!args.term) {
                throw "args.term is required";
            }

            return entityResource.search(args.term, "Document").then(function (data) {

                _.each(data, function(el) {
                    configureContentResult(el);
                });

                var results = (args.results && angular.isArray(args.results)) ? args.results : [];
                
                args.results.push({
                    icon: "icon-document",
                    editor: "content/content/edit/",
                    matches: data
                });
                
                return results;
            });
        },
        searchMedia: function(args) {

            if (!args.term) {
                throw "args.term is required";
            }

            return entityResource.search(args.term, "Media").then(function (data) {

                _.each(data, function(el) {
                    configureMediaResult(el);
                });

                var results = (args.results && angular.isArray(args.results)) ? args.results : [];

                args.results.push({
                    icon: "icon-picture",
                    editor: "media/media/edit/",
                    matches: data
                });
                
                return results;
            });
        },
        searchAll: function (args) {
            
            if (!args.term) {
                throw "args.term is required";
            }

            return entityResource.searchAll(args.term).then(function (data) {

                var results = (args.results && angular.isArray(args.results)) ? args.results : [];

                _.each(data, function(resultByType) {
                    switch(resultByType.type) {
                        case "Document":
                            _.each(resultByType.results, function (el) {
                                configureContentResult(el);
                            });
                            results.push({
                                type: resultByType.type,
                                icon: "icon-document",
                                editor: "content/content/edit/",
                                matches: resultByType.results
                            });
                            break;
                        case "Media":
                            _.each(resultByType.results, function (el) {
                                configureMediaResult(el);
                            });
                            results.push({
                                type: resultByType.type,
                                icon: "icon-picture",
                                editor: "media/media/edit/",
                                matches: resultByType.results
                            });
                            break;
                        case "Member":
                            _.each(resultByType.results, function (el) {
                                configureMemberResult(el);
                            });
                            results.push({
                                type: resultByType.type,
                                icon: "icon-user",
                                editor: "member/member/edit/",
                                matches: resultByType.results
                            });
                            break;
                    }
                });

                return results;
            });
            
        },

        setCurrent: function(sectionAlias) {
            currentSection = sectionAlias;
        }
    };
});