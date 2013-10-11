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

function startupLatestEditsController($scope) {
    
}
angular.module("umbraco").controller("Umbraco.Dashboard.StartupLatestEditsController", startupLatestEditsController);

function MediaFolderBrowserDashboardController($rootScope, $scope, assetsService, $routeParams, $timeout, $element, $location, umbRequestHelper, mediaResource, imageHelper) {
        var dialogOptions = $scope.$parent.dialogOptions;

        $scope.filesUploading = [];
        $scope.options = {
            url: umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostAddFile"),
            autoUpload: true,
            disableImageResize: /Android(?!.*Chrome)|Opera/
            .test(window.navigator.userAgent),
            previewMaxWidth: 200,
            previewMaxHeight: 200,
            previewCrop: true,
            formData:{
                currentFolder: -1
            }
        };


        $scope.loadChildren = function(){
            mediaResource.getChildren(-1)
                .then(function(data) {
                    $scope.images = data.items;
                });    
        };

        $scope.$on('fileuploadstop', function(event, files){
            $scope.loadChildren($scope.options.formData.currentFolder);
            $scope.queue = [];
            $scope.filesUploading = [];
        });

        $scope.$on('fileuploadprocessalways', function(e,data) {
            var i;
            $scope.$apply(function() {
                $scope.filesUploading.push(data.files[data.index]);
            });
        });

        // All these sit-ups are to add dropzone area and make sure it gets removed if dragging is aborted! 
        $scope.$on('fileuploaddragover', function(event, files) {
            if (!$scope.dragClearTimeout) {
                $scope.$apply(function() {
                    $scope.dropping = true;
                });
            } else {
                $timeout.cancel($scope.dragClearTimeout);
            }
            $scope.dragClearTimeout = $timeout(function () {
                $scope.dropping = null;
                $scope.dragClearTimeout = null;
            }, 300);
        });
        
        //init load
        $scope.loadChildren();
}
angular.module("umbraco").controller("Umbraco.Dashboard.MediaFolderBrowserDashboardController", MediaFolderBrowserDashboardController);


function ChangePasswordDashboardController($scope, xmlhelper, $log, userResource, formHelper) {
    //this is the model we will pass to the service
    $scope.profile = {};

    $scope.changePassword = function(p) {

        if (formHelper.submitForm({ scope: $scope })) {
            userResource.changePassword(p.oldPassword, p.newPassword).then(function() {

                formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                //TODO: This is temporary - server validation will work automatically with the val-server directives.
                $scope.passwordForm.$setValidity(true);
            }, function () {
                //TODO: This is temporary - server validation will work automatically with the val-server directives.
                //this only happens if there is a wrong oldPassword sent along
                $scope.passwordForm.oldpass.$setValidity("oldPassword", false);
            });
        }
    };
}
angular.module("umbraco").controller("Umbraco.Dashboard.StartupChangePasswordController", ChangePasswordDashboardController);