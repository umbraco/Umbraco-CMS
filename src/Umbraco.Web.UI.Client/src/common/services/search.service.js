angular.module('umbraco.services')
.factory('searchService', function ($q, $log, entityResource, contentResource) {
	var m = {results: []};
	var service = {
		results: m,

		searchMembers: function(args){
			entityResource.search(args.term, "Member").then(function(data){

				_.each(data, function(el){
					el.menuUrl = "UmbracoTrees/MemberTree/GetMenu?id=" + el.id + "&application=member";
					el.metaData = {treeAlias: "member"};
					el.title = el.Fields.nodeName;
					el.subTitle = el.Fields.email;
				});

				args.results.push({
					icon: "icon-user",
					editor: "member/member/edit/",
					matches: data
				});
			});
		},
		searchContent: function(args){
			entityResource.search(args.term, "Document").then(function(data){

				_.each(data, function(el){
					el.menuUrl = "UmbracoTrees/ContentTree/GetMenu?id=" + el.id + "&application=content";
					el.metaData = {treeAlias: "content"};
					el.title = el.Fields.nodeName;

					contentResource.getNiceUrl(el.Id).then(function(url){
						el.subTitle = url;
					});
				});

				args.results.push({
					icon: "icon-document",
					editor: "content/content/edit/",
					matches: data
				});
			});
		},
		searchMedia: function(args){
			entityResource.search(args.term, "Media").then(function(data){

				_.each(data, function(el){
					el.menuUrl = "UmbracoTrees/MediaTree/GetMenu?id=" + el.id + "&application=media";
					el.metaData = {treeAlias: "media"};
					el.title = el.Fields.nodeName;
				});

				args.results.push({
					icon: "icon-picture",
					editor: "media/media/edit/",
					matches: data
				});
			});
		},
		search: function(term){
			m.results.length = 0;

			service.searchMedia({term:term, results:m.results});
			service.searchContent({term:term, results:m.results});
			service.searchMembers({term:term, results:m.results});
		},
		
		setCurrent: function(sectionAlias){
			currentSection = sectionAlias;	
		}
	};

	return service;
});