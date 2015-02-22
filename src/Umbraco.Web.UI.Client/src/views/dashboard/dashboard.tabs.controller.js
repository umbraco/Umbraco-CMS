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


function FormsController($scope, $route, $cookieStore, packageResource) {
    $scope.installForms = function(){
        $scope.state = "Installng package";
        packageResource
            .fetch("CD44CF39-3D71-4C19-B6EE-948E1FAF0525")
            .then(function(pack){
              $scope.state = "importing";
              return packageResource.import(pack);
            }, $scope.error)
            .then(function(pack){
              $scope.state = "Installing";
              return packageResource.installFiles(pack);
            }, $scope.error)
            .then(function(pack){
              $scope.state = "Restarting, please hold...";
              return packageResource.installData(pack);
            }, $scope.error)
            .then(function(pack){
              $scope.state = "All done, your browser will now refresh";
              return packageResource.cleanUp(pack);
            }, $scope.error)
            .then($scope.complete, $scope.error);
    };

    $scope.complete = function(result){
        var url = window.location.href + "?init=true";
        $cookieStore.put("umbPackageInstallId", result.packageGuid); 
        window.location.reload(true);
    };

    $scope.error = function(err){
        $scope.state = undefined;
        $scope.error = err;
    };


    function Video_player (videoId) {
      // Get dom elements
      this.container      = document.getElementById(videoId);
      this.video          = this.container.getElementsByTagName('video')[0];

      //Create controls
      this.controls = document.createElement('div');
      this.controls.className="video-controls";

      this.seek_bar = document.createElement('input');
      this.seek_bar.className="seek-bar";
      this.seek_bar.type="range";
      this.seek_bar.setAttribute('value', '0');

      this.loader = document.createElement('div');
      this.loader.className="loader";

      this.progress_bar = document.createElement('span');
      this.progress_bar.className="progress-bar";

      // Insert controls
      this.controls.appendChild(this.seek_bar);
      this.container.appendChild(this.controls);
      this.controls.appendChild(this.loader);
      this.loader.appendChild(this.progress_bar);
    }


    Video_player.prototype
      .seeking = function() {
        // get the value of the seekbar (hidden input[type="range"])
        var time = this.video.duration * (this.seek_bar.value / 100);

        // Update video to seekbar value
        this.video.currentTime = time;
      };

    // Stop video when user initiates seeking
    Video_player.prototype
      .start_seek = function() {
        this.video.pause();
      };

    // Start video when user stops seeking
    Video_player.prototype
      .stop_seek = function() {
        this.video.play();
      };

    // Update the progressbar (span.loader) according to video.currentTime
    Video_player.prototype
      .update_progress_bar = function() {
        // Get video progress in %
        var value = (100 / this.video.duration) * this.video.currentTime;

        // Update progressbar
        this.progress_bar.style.width = value + '%';
      };

    // Bind progressbar to mouse when seeking
    Video_player.prototype
      .handle_mouse_move = function(event) {
        // Get position of progressbar relative to browser window
        var pos = this.progress_bar.getBoundingClientRect().left;

        // Make sure event is reckonized cross-browser
        event = event || window.event;

        // Update progressbar
        this.progress_bar.style.width = (event.clientX - pos) + "px";
      };

    // Eventlisteners for seeking
    Video_player.prototype
      .video_event_handler = function(videoPlayer, interval) {
        // Update the progress bar
        var animate_progress_bar = setInterval(function () {
              videoPlayer.update_progress_bar();
            }, interval);

        // Fire when input value changes (user seeking)
        videoPlayer.seek_bar
          .addEventListener("change", function() {
              videoPlayer.seeking();
          });

        // Fire when user clicks on seekbar
        videoPlayer.seek_bar
          .addEventListener("mousedown", function (clickEvent) {
              // Pause video playback
              videoPlayer.start_seek();

              // Stop updating progressbar according to video progress
              clearInterval(animate_progress_bar);

              // Update progressbar to where user clicks
              videoPlayer.handle_mouse_move(clickEvent);

              // Bind progressbar to cursor
              window.onmousemove = function(moveEvent){
                videoPlayer.handle_mouse_move(moveEvent);
              };
          });

        // Fire when user releases seekbar
        videoPlayer.seek_bar
          .addEventListener("mouseup", function () {

              // Unbind progressbar from cursor
              window.onmousemove = null;

              // Start video playback
              videoPlayer.stop_seek();

              // Animate the progressbar
              animate_progress_bar = setInterval(function () {
                  videoPlayer.update_progress_bar();
              }, interval);
          });
      };


    var videoPlayer = new Video_player('video_1');
    videoPlayer.video_event_handler(videoPlayer, 17);
}

angular.module("umbraco").controller("Umbraco.Dashboard.FormsDashboardController", FormsController);

function startupLatestEditsController($scope) {

}
angular.module("umbraco").controller("Umbraco.Dashboard.StartupLatestEditsController", startupLatestEditsController);

function MediaFolderBrowserDashboardController($rootScope, $scope, assetsService, $routeParams, $timeout, $element, $location, umbRequestHelper,navigationService, mediaResource, $cookies) {
        var dialogOptions = $scope.dialogOptions;

        $scope.filesUploading = [];
        $scope.nodeId = -1;

        $scope.onUploadComplete = function () {
            navigationService.reloadSection("media");
        }

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
