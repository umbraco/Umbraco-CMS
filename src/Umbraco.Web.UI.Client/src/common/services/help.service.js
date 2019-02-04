angular.module('umbraco.services')
	.factory('helpService', function ($http, $q, umbRequestHelper, dashboardResource) {
		var helpTopics = {};

		var defaultUrl = "rss/help";
		var tvUrl = "feeds/help";

		function getCachedHelp(url){
			if(helpTopics[url]){
				return helpTopics[cacheKey];
			}else{
				return null;
			}
		}

		function setCachedHelp(url, data){
			helpTopics[url] = data;
		}

		function fetchUrl(site, url){
			var deferred = $q.defer();
			var found = getCachedHelp(url);

			if(found){
				deferred.resolve(found);
			}else{
				dashboardResource.getRemoteXmlData(site, url).then(function (data) {
					var feed = $(data.data);
					var topics = [];

					$('item', feed).each(function (i, item) {
						var topic = {};
						topic.thumbnail = $(item).find('thumbnail').attr('url');
						topic.title = $("title", item).text();
						topic.link = $("guid", item).text();
						topic.description = $("description", item).text();
						topics.push(topic);
					});

					setCachedHelp(topics);
					deferred.resolve(topics);

				},
				function (exception) {
					console.error('ex from remote data', exception);
				});

			}

			return deferred.promise;
		}

		var service = {
			findHelp: function (args) {
				var url = service.getUrl(defaultUrl, args);
				return fetchUrl('OUR', url);
			},

			findVideos: function (args) {
				var url = service.getUrl(tvUrl, args);
				return fetchUrl('TV', url);
			},

			getContextHelpForPage: function (section, tree, baseurl) {

			    var qs = "?section=" + section + "&tree=" + tree;

			    if (tree) {
			        qs += "&tree=" + tree;
			    }

			    if (baseurl) {
			        qs += "&baseurl=" + encodeURIComponent(baseurl);
			    }

			    var url = umbRequestHelper.getApiUrl(
                        "helpApiBaseUrl",
                        "GetContextHelpForPage" + qs);

			    return umbRequestHelper.resourcePromise(
                        $http.get(url), "Failed to get lessons content");
			},

			getUrl: function(url, args){
				return url + "?" + $.param(args);
			}
		};

		return service;

	});
