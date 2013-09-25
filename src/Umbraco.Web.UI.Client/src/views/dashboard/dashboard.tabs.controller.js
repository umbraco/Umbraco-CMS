function startUpVideosDashboardController($scope, xmlhelper, $log, $http) {
    $scope.videos = [];
    $scope.init = function(url){
        var proxyUrl = "dashboard/feedproxy.aspx?url=" + url; 
        $http.get(proxyUrl).then(function(data){
              var feed = $(data.data);
              $('item', feed).each(function (i, item) {
                  var video = {};
                  video.thumbnail = $(item).find('thumbnail').attr('url');
                  video.title = $("title", item).text();
                  video.link = $("guid", item).text();
                  $scope.videos.push(video);      
              });
        });
    };
}
angular.module("umbraco").controller("Umbraco.Dashboard.StartupVideosController", startUpVideosDashboardController);


function MediaFolderBrowserDashboardController($scope, xmlhelper, $log, userResource) {
    //this is the model we will pass to the service
    $scope.profile = {};

    $scope.changePassword = function (p) {   
        userResource.changePassword(p.oldPassword, p.newPassword).then(function () {
            $scope.passwordForm.$setValidity(true);
        }, function () {
            //this only happens if there is a wrong oldPassword sent along
            $scope.passwordForm.oldpass.$setValidity("oldPassword", false);
        });
    }
}
angular.module("umbraco").controller("Umbraco.Dashboard.MediaFolderBrowserDashboardController", MediaFolderBrowserDashboardController);


function ChangePasswordDashboardController($scope, xmlhelper, $log, userResource) {
    //this is the model we will pass to the service
    $scope.profile = {};

    $scope.changePassword = function (p) {   
        userResource.changePassword(p.oldPassword, p.newPassword).then(function () {
            $scope.passwordForm.$setValidity(true);
        }, function () {
            //this only happens if there is a wrong oldPassword sent along
            $scope.passwordForm.oldpass.$setValidity("oldPassword", false);
        });
    }
}
angular.module("umbraco").controller("Umbraco.Dashboard.StartupChangePasswordController", ChangePasswordDashboardController);