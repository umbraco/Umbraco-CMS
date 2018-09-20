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


function startUpDynamicContentController($timeout, $scope, dashboardResource, assetsService, tourService, eventsService) {
    var vm = this;
    var evts = [];

    vm.loading = true;
    vm.showDefault = false;
    
    vm.startTour = startTour;

    function onInit() {
        // load tours
        tourService.getGroupedTours().then(function(groupedTours) {
            vm.tours = groupedTours;
        });
    }

    function startTour(tour) {
        tourService.startTour(tour);
    }

    // default dashboard content
    vm.defaultDashboard = {
        infoBoxes: [
            {
                title: "Documentation",
                description: "Find the answers to your Umbraco questions",
                url: "https://our.umbraco.com/documentation/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=documentation/"
            },
            {
                title: "Community",
                description: "Find the answers or ask your Umbraco questions",
                url: "https://our.umbraco.com/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=our_forum"
            },
            {
                title: "Umbraco.tv",
                description: "Tutorial videos (some are free, some are on subscription)",
                url: "https://umbraco.tv/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=tutorial_videos"
            },
            {
                title: "Training",
                description: "Real-life training and official Umbraco certifications",
                url: "https://umbraco.com/training/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=training"
            }
        ],
        articles: [
            {
                title: "Umbraco.TV - Learn from the source!",
                description: "Umbraco.TV will help you go from zero to Umbraco hero at a pace that suits you. Our easy to follow online training videos will give you the fundamental knowledge to start building awesome Umbraco websites.",
                img: "views/dashboard/default/umbracotv.jpg",
                url: "https://umbraco.tv/?utm_source=core&utm_medium=dashboard&utm_content=image&utm_campaign=tv",
                altText: "Umbraco.TV - Hours of Umbraco Video Tutorials",
                buttonText: "Visit Umbraco.TV"
            },
            {
                title: "Our Umbraco - The Friendliest Community",
                description: "Our Umbraco - the official community site is your one stop for everything Umbraco. Whether you need a question answered or looking for cool plugins, the world's best and friendliest community is just a click away.",
                img: "views/dashboard/default/ourumbraco.jpg",
                url: "https://our.umbraco.com/?utm_source=core&utm_medium=dashboard&utm_content=image&utm_campaign=our",
                altText: "Our Umbraco",
                buttonText: "Visit Our Umbraco"
            }
        ]
    };

    evts.push(eventsService.on("appState.tour.complete", function (name, completedTour) {
        $timeout(function(){
            angular.forEach(vm.tours, function (tourGroup) {
                angular.forEach(tourGroup, function (tour) {
                    if(tour.alias === completedTour.alias) {
                        tour.completed = true;
                    }
                });
            });
        });
    }));
    
    //proxy remote css through the local server
    assetsService.loadCss(dashboardResource.getRemoteDashboardCssUrl("content"), $scope);
    dashboardResource.getRemoteDashboardContent("content").then(
        function (data) {

            vm.loading = false;

            //test if we have received valid data
            //we capture it like this, so we avoid UI errors - which automatically triggers ui based on http response code
            if (data && data.sections) {
                vm.dashboard = data;
            } else{
                vm.showDefault = true;
            }

        },

        function (exception) {
            console.error(exception);
            vm.loading = false;
            vm.showDefault = true;
        });

    
    onInit();

}

angular.module("umbraco").controller("Umbraco.Dashboard.StartUpDynamicContentController", startUpDynamicContentController);


function FormsController($scope, $route, $cookies, packageResource, localizationService) {

    var labels = {};
    var labelKeys = [
        "packager_installStateDownloading",
        "packager_installStateImporting",
        "packager_installStateInstalling",
        "packager_installStateRestarting",
        "packager_installStateComplete"
    ];

    localizationService.localizeMany(labelKeys).then(function(values){
        labels.installStateDownloading = values[0];
        labels.installStateImporting = values[1];
        labels.installStateInstalling = values[2];
        labels.installStateRestarting = values[3];
        labels.installStateComplete = values[4];
    });

    $scope.installForms = function(){
        $scope.state = labels.installStateDownloading;
        packageResource
            .fetch("CD44CF39-3D71-4C19-B6EE-948E1FAF0525")
            .then(function(pack) {
                    $scope.state = labels.installStateImporting;
                    return packageResource.import(pack);
                },
                $scope.error)
            .then(function(pack) {
                $scope.state = labels.installStateInstalling;
                    return packageResource.installFiles(pack);
                },
                $scope.error)
            .then(function(pack) {
                $scope.state = labels.installStateRestarting;
                    return packageResource.installData(pack);
                },
                $scope.error)
            .then(function(pack) {
                $scope.state = installStateComplete;
                    return packageResource.cleanUp(pack);
                },
                $scope.error)
            .then($scope.complete, $scope.error);
    };

    $scope.complete = function(result){
        var url = window.location.href + "?init=true";
        $cookies.putObject("umbPackageInstallId", result.packageGuid);
        window.location.reload(true);
    };

    $scope.error = function(err){
        $scope.state = undefined;
        $scope.error = err;
        //This will return a rejection meaning that the promise change above will stop
        return $q.reject();
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

function MediaFolderBrowserDashboardController($rootScope, $scope, $location, contentTypeResource, userService) {

    var currentUser = {};

    userService.getCurrentUser().then(function (user) {

        currentUser = user;

        // check if the user has access to the root which they will require to see this dashboard
        if (currentUser.startMediaIds.indexOf(-1) >= 0) {

            //get the system media listview
            contentTypeResource.getPropertyTypeScaffold(-96)
                .then(function(dt) {

                    $scope.fakeProperty = {
                        alias: "contents",
                        config: dt.config,
                        description: "",
                        editor: dt.editor,
                        hideLabel: true,
                        id: 1,
                        label: "Contents:",
                        validation: {
                            mandatory: false,
                            pattern: null
                        },
                        value: "",
                        view: dt.view
                    };

            });

        } else if (currentUser.startMediaIds.length > 0){
            // redirect to start node
            $location.path("/media/media/edit/" + (currentUser.startMediaIds.length === 0 ? -1 : currentUser.startMediaIds[0]));
        }

    });

}
angular.module("umbraco").controller("Umbraco.Dashboard.MediaFolderBrowserDashboardController", MediaFolderBrowserDashboardController);
