angular.module('umbraco.services')
	.factory('helpService', function ($http, $q){
		var helpTopics = {};

		var defaultUrl = "http://our.umbraco.org/rss/help";
		var tvUrl = "http://umbraco.tv/feeds/help";

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

		function fetchUrl(url){
			var deferred = $q.defer();
			var found = getCachedHelp(url);

			if(found){
				deferred.resolve(found);
			}else{

				var proxyUrl = "dashboard/feedproxy.aspx?url=" + url; 
				$http.get(proxyUrl).then(function(data){
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
				});
			}

			return deferred.promise;
		}



		var service = {
			findHelp: function (args) {
				var url = service.getUrl(defaultUrl, args);
				return fetchUrl(url);
			},

			findVideos: function (args) {
				var url = service.getUrl(tvUrl, args);
				return fetchUrl(url);
			},

			getUrl: function(url, args){
				return url + "?" + $.param(args);
			}
		};

		return service;

	});