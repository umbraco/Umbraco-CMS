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

function MediaFolderBrowserDashboardController($rootScope, $scope, assetsService, $routeParams, $timeout, $element, $location, umbRequestHelper,navigationService, mediaResource, $cookies) {
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
            navigationService.reloadSection("media");
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


function ChangePasswordDashboardController($scope, xmlhelper, $log, currentUserResource, formHelper) {

    //create the initial model for change password property editor
    $scope.changePasswordModel = {
        alias: "_umb_password",
        view: "changepassword",
        config: {},
        value: {}
    };

    //go get the config for the membership provider and add it to the model
    currentUserResource.getMembershipProviderConfig().then(function(data) {
        $scope.changePasswordModel.config = data;
        //ensure the hasPassword config option is set to true (the user of course has a password already assigned)
        //this will ensure the oldPassword is shown so they can change it
        $scope.changePasswordModel.config.hasPassword = true;
        $scope.changePasswordModel.config.disableToggle = true;
    });

    ////this is the model we will pass to the service
    //$scope.profile = {};

    $scope.changePassword = function() {

        if (formHelper.submitForm({ scope: $scope })) {
            currentUserResource.changePassword($scope.changePasswordModel.value).then(function(data) {

                //if the password has been reset, then update our model
                if (data.value) {
                    $scope.changePasswordModel.value.generatedPassword = data.value;
                }

                formHelper.resetForm({ scope: $scope, notifications: data.notifications });
                
            }, function (err) {
                
                formHelper.handleError(err);
                
            });
        }
    };
}
angular.module("umbraco").controller("Umbraco.Dashboard.StartupChangePasswordController", ChangePasswordDashboardController);