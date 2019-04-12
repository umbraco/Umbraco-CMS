
namespace umbraco.services {
    export class searchSerice {

        public constructor(
            protected $q, 
            protected $log, 
            protected entityResource, 
            protected contentResource, 
            protected umbRequestHelper, 
            protected $injector, 
            protected searchResultFormatter : umbraco.services.searchResultFormatter) {
            
        }

        public searchMembers(args: any) {

            if (!args.term) {
                throw "args.term is required";
            }

            return this.entityResource.search(args.term, "Member", args.searchFrom).then(function (data) {
                _.each(data, function (item: umbraco.models.search.member) {
                    this.searchResultFormatter.configureMemberResult(item);
                });
                return data;
            });
        }

        public searchContent(args) {

            if (!args.term) {
                throw "args.term is required";
            }

            return this.entityResource.search(args.term, "Document", args.searchFrom, args.canceler).then(function (data) {
                _.each(data, function (item) {
                    this.searchResultFormatter.configureContentResult(item);
                });
                return data;
            });
        }

        public searchMedia(args) {

            if (!args.term) {
                throw "args.term is required";
            }

            return this.entityResource.search(args.term, "Media", args.searchFrom).then(function (data) {
                _.each(data, function (item) {
                    this.searchResultFormatter.configureMediaResult(item);
                });
                return data;
            });
        }

        public searchAll(args) {

            if (!args.term) {
                throw "args.term is required";
            }

            return this.entityResource.searchAll(args.term, args.canceler).then(function (data) {

                _.each(data, function (resultByType) {

                    //we need to format the search result data to include things like the subtitle, urls, etc...
                    // this is done with registered angular services as part of the SearchableTreeAttribute, if that 
                    // is not found, than we format with the default formatter
                    var formatterMethod = this.searchResultFormatter.configureDefaultResult;
                    //check if a custom formatter is specified...
                    if (resultByType.jsSvc) {
                        var searchFormatterService = this.$injector.get(resultByType.jsSvc);
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
                    _.each(resultByType.results, function (item) {
                        formatterMethod.apply(this, [item, resultByType.treeAlias, resultByType.appAlias]);
                    });
                    
                });

                return data;
            });

        }

        public setCurrent(sectionAlias: string) {
            let currentSection = sectionAlias;
        }

    }
}

angular.module('umbraco.services').service('searchService', umbraco.services.searchSerice);