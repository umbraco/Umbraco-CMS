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

function ChangePasswordDashboardController($scope, xmlhelper, $log, userService) {
    //this is the model we will pass to the service
    $scope.profile = {};
    $scope.changePassword = function (p) {
        userService.changePassword(p.oldPassword, p.newPassword).then(function () {
            //changed
        }, function () {
            //this only happens if there is a wrong oldPassword sent along
            $scope.passwordForm.oldPass.$setValidity("oldPassword", false);
        });

    }
}

angular.module("umbraco").controller("Umbraco.Dashboard.StartupChangePasswordController", ChangePasswordDashboardController);