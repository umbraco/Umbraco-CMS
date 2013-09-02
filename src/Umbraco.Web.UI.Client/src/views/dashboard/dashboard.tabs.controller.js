function startUpVideosDashboardController($scope, xmlhelper, $log, $http) {


	//xmlHelper.parseFeed("http://umbraco.org/feeds/videos/getting-started").then(function(feed){
		
	//});
   $scope.videos = [];
   $http.get("dashboard/feedproxy.aspx?url=http://umbraco.org/feeds/videos/getting-started").then(function(data){
    	var feed = $(data.data);
    	$('item', feed).each(function (i, item) {
   				var video = {};
   				video.thumbnail = $(item).find('thumbnail').attr('url');
   				video.title = $("title", item).text();
   				video.link = $("guid", item).text();
   				$scope.videos.push(video);  		
    	});
   });
}

angular.module("umbraco").controller("Umbraco.Dashboard.StartupVideosController", startUpVideosDashboardController);